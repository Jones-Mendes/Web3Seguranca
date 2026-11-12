using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Web3Seguranca
{
    /// <summary>
    /// SOMENTE PARA TESTES NO EDITOR.
    /// Na aba Game, clique num card com o botão ESQUERDO do mouse e arraste.
    /// Ao soltar, o card cai — solte em cima da lixeira ou da bandeja verde.
    /// No build para o óculos este script não faz nada.
    /// </summary>
    public class ArrastarComMouse : MonoBehaviour
    {
        [Tooltip("Altura (em metros) em que o card flutua enquanto é arrastado.")]
        [SerializeField] float alturaDeArraste = 1.3f;

        MessageCard card;
        Rigidbody rb;
        Plane plano;

#if UNITY_EDITOR
        void Update()
        {
            var cam = Camera.main;
            if (!cam) return;

            bool apertou, segurando;
            Vector2 posMouse;
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse == null) return;
            apertou   = mouse.leftButton.wasPressedThisFrame;
            segurando = mouse.leftButton.isPressed;
            posMouse  = mouse.position.ReadValue();
#else
            apertou   = Input.GetMouseButtonDown(0);
            segurando = Input.GetMouseButton(0);
            posMouse  = Input.mousePosition;
#endif
            Ray raio = cam.ScreenPointToRay(posMouse);

            // Começar a arrastar
            if (apertou && Physics.Raycast(raio, out var hit, 10f, ~0, QueryTriggerInteraction.Ignore))
            {
                card = hit.collider.GetComponentInParent<MessageCard>();
                if (card && !card.Resolvido)
                {
                    rb = card.GetComponent<Rigidbody>();
                    rb.isKinematic = true;
                    plano = new Plane(Vector3.up, new Vector3(0f, alturaDeArraste, 0f));
                }
                else card = null;
            }

            if (!card) return;

            // A zona já recebeu o card durante o arraste
            if (card.Resolvido) { card = null; return; }

            if (segurando)
            {
                if (plano.Raycast(raio, out float distancia))
                    card.transform.position = raio.GetPoint(distancia);
            }
            else
            {
                // Soltou: o card cai e as zonas detectam a queda
                rb.isKinematic = false;
                card = null;
            }
        }
#endif
    }
}