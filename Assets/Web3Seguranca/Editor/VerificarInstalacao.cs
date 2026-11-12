#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Web3Seguranca.EditorTools
{
    /// <summary>
    /// Menu: Web3 Seguranca > Verificar instalação
    /// Confere pacotes, TextMeshPro, samples e configuração do OpenXR (etapa 3 do guia).
    /// Usa reflexão para compilar mesmo quando algum pacote ainda não está instalado.
    /// Coloque este arquivo dentro de uma pasta chamada "Editor".
    /// </summary>
    public static class VerificarInstalacao
    {
        static readonly (string id, string nome, string minima)[] Pacotes =
        {
            ("com.unity.xr.interaction.toolkit",      "XR Interaction Toolkit", "3.0.0"),
            ("com.unity.xr.hands",                    "XR Hands",               "1.3.0"),
            ("com.unity.xr.openxr",                   "OpenXR Plugin",          "1.9.0"),
            ("com.unity.xr.management",               "XR Plugin Management",   "4.4.0"),
            ("com.unity.inputsystem",                 "Input System",           "1.7.0"),
            ("com.unity.render-pipelines.universal",  "URP",                    null),
            ("com.unity.probuilder",                  "ProBuilder",             null),
        };

        // (nome mostrado na tela do OpenXR, nome da classe da feature)
        static readonly (string nome, string tipo)[] FeaturesAndroid =
        {
            ("Meta Quest Support",              "MetaQuestFeature"),
            ("Hand Tracking Subsystem",         "HandTracking"),
            ("Meta Hand Tracking Aim",          "MetaHandTrackingAim"),
            ("Oculus Touch Controller Profile", "OculusTouchControllerProfile"),
            ("Hand Interaction Profile",        "HandInteractionProfile"),
        };

        static readonly (string nome, string tipo)[] FeaturesPC =
        {
            ("Hand Tracking Subsystem",         "HandTracking"),
            ("Meta Hand Tracking Aim",          "MetaHandTrackingAim"),
            ("Oculus Touch Controller Profile", "OculusTouchControllerProfile"),
            ("Hand Interaction Profile",        "HandInteractionProfile"),
        };

        static ListRequest pedido;

        [MenuItem("Web3 Seguranca/Verificar instalação")]
        static void Verificar()
        {
            pedido = Client.List(true, true);
            EditorApplication.update += AguardarLista;
        }

        static void AguardarLista()
        {
            if (!pedido.IsCompleted) return;
            EditorApplication.update -= AguardarLista;

            var r = new StringBuilder();
            int problemas = 0;

            // ---------- Pacotes ----------
            r.AppendLine("PACOTES");
            if (pedido.Status != StatusCode.Success)
            {
                r.AppendLine("  [ERRO] Não foi possível listar os pacotes: " + pedido.Error?.message);
                problemas++;
            }
            else
            {
                foreach (var (id, nome, minima) in Pacotes)
                {
                    var p = pedido.Result.FirstOrDefault(x => x.name == id);
                    if (p == null) { r.AppendLine($"  [FALTA] {nome}"); problemas++; continue; }
                    if (minima != null && Comparar(p.version, minima) < 0)
                    {
                        r.AppendLine($"  [ANTIGO] {nome} {p.version} (recomendado {minima} ou mais)");
                        problemas++;
                    }
                    else r.AppendLine($"  [OK] {nome} {p.version}");
                }
            }

            // ---------- TextMeshPro ----------
            r.AppendLine("\nTEXTMESHPRO");
            bool tmp = AssetDatabase.FindAssets("TMP Settings").Length > 0;
            r.AppendLine(tmp ? "  [OK] TMP Essential Resources importados"
                             : "  [FALTA] Window > TextMeshPro > Import TMP Essential Resources");
            if (!tmp) problemas++;

            // ---------- Samples ----------
            r.AppendLine("\nSAMPLES DO XRI");
            problemas += Sample(r, "Starter Assets");
            problemas += Sample(r, "Hands Interaction Demo");

            // ---------- XR / OpenXR ----------
            r.AppendLine("\nXR PLUG-IN MANAGEMENT");
            problemas += VerificarLoader(r, BuildTargetGroup.Android, "Android");
            problemas += VerificarLoader(r, BuildTargetGroup.Standalone, "PC (Quest Link)");

            r.AppendLine("\nOPENXR FEATURES");
            problemas += VerificarFeatures(r, BuildTargetGroup.Android, "Android", FeaturesAndroid);
            problemas += VerificarFeatures(r, BuildTargetGroup.Standalone, "PC", FeaturesPC);

            // ---------- Resultado ----------
            string titulo = problemas == 0
                ? "Tudo certo! A etapa 3 está concluída."
                : $"{problemas} item(ns) para corrigir.";
            r.Insert(0, titulo + "\n\n");
            r.AppendLine("\nDepois, confira também Project Validation (XR Plug-in Management) sem itens vermelhos.");

            if (problemas == 0) Debug.Log(r.ToString()); else Debug.LogWarning(r.ToString());

            var pendencias = r.ToString().Split('\n')
                .Where(l => l.TrimStart().StartsWith("[") && !l.Contains("[OK]"))
                .Select(l => l.Trim());
            string janela = problemas == 0
                ? titulo
                : titulo + "\n\n" + string.Join("\n", pendencias) + "\n\n(Relatório completo no Console.)";
            EditorUtility.DisplayDialog("Verificação da instalação", janela, "OK");
        }

        static int Sample(StringBuilder r, string nome)
        {
            bool ok = AssetDatabase.IsValidFolder("Assets/Samples") &&
                      AssetDatabase.GetSubFolders("Assets/Samples")
                          .SelectMany(TodasSubpastas)
                          .Any(p => p.EndsWith("/" + nome));
            r.AppendLine(ok ? $"  [OK] {nome}" : $"  [FALTA] {nome} (Package Manager > XR Interaction Toolkit > Samples)");
            return ok ? 0 : 1;
        }

        static System.Collections.Generic.IEnumerable<string> TodasSubpastas(string pasta)
        {
            yield return pasta;
            foreach (var sub in AssetDatabase.GetSubFolders(pasta))
                foreach (var s in TodasSubpastas(sub)) yield return s;
        }

        static int VerificarLoader(StringBuilder r, BuildTargetGroup grupo, string rotulo)
        {
            var tipoPorAlvo = Tipo("UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget");
            if (tipoPorAlvo == null) { r.AppendLine($"  [FALTA] XR Plugin Management não encontrado"); return 1; }

            var metodo = tipoPorAlvo.GetMethod("XRGeneralSettingsForBuildTarget", BindingFlags.Public | BindingFlags.Static);
            var geral = metodo?.Invoke(null, new object[] { grupo });
            var manager = geral?.GetType().GetProperty("Manager")?.GetValue(geral);
            var loaders = manager?.GetType().GetProperty("activeLoaders")?.GetValue(manager) as IEnumerable;

            bool ok = loaders != null && loaders.Cast<object>().Any(l => l != null && l.GetType().Name.Contains("OpenXR"));
            r.AppendLine(ok ? $"  [OK] OpenXR ativo em {rotulo}"
                            : $"  [FALTA] Marque OpenXR na aba {rotulo} de XR Plug-in Management");
            return ok ? 0 : 1;
        }

        static int VerificarFeatures(StringBuilder r, BuildTargetGroup grupo, string rotulo, (string nome, string tipo)[] esperadas)
        {
            var tipoSettings = Tipo("UnityEngine.XR.OpenXR.OpenXRSettings");
            if (tipoSettings == null) { r.AppendLine($"  [FALTA] OpenXR Plugin não encontrado"); return 1; }

            var settings = tipoSettings.GetMethod("GetSettingsForBuildTargetGroup", BindingFlags.Public | BindingFlags.Static)
                                       ?.Invoke(null, new object[] { grupo });
            var getFeatures = tipoSettings.GetMethods()
                .FirstOrDefault(m => m.Name == "GetFeatures" && !m.IsGenericMethod && m.GetParameters().Length == 0);
            var features = (getFeatures?.Invoke(settings, null) as Array)?.Cast<object>().ToArray() ?? Array.Empty<object>();

            int faltando = 0;
            foreach (var (nome, tipo) in esperadas)
            {
                var f = features.FirstOrDefault(x => x != null &&
                    (x.GetType().Name == tipo ||
                     NomeDaFeature(x).IndexOf(nome, StringComparison.OrdinalIgnoreCase) >= 0));
                bool ativa = f != null && (bool)(f.GetType().GetProperty("enabled")?.GetValue(f) ?? false);
                if (ativa) r.AppendLine($"  [OK] {rotulo}: {nome}");
                else
                {
                    r.AppendLine(f == null ? $"  [FALTA] {rotulo}: {nome} (não existe nesta versão/pacote)"
                                           : $"  [DESLIGADA] {rotulo}: {nome}");
                    faltando++;
                }
            }
            return faltando;
        }

        static string NomeDaFeature(object f)
        {
            FieldInfo campo = null;
            for (var t = f.GetType(); t != null && campo == null; t = t.BaseType)
                campo = t.GetField("nameUi", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return (campo?.GetValue(f) as string) ?? (f as UnityEngine.Object)?.name ?? "";
        }

        static Type Tipo(string nomeCompleto) =>
            AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(nomeCompleto, false))
                .FirstOrDefault(t => t != null);

        static int Comparar(string versao, string minima)
        {
            Version Ler(string v) => Version.TryParse(v.Split('-')[0], out var x) ? x : new Version(0, 0);
            return Ler(versao).CompareTo(Ler(minima));
        }
    }
}
#endif