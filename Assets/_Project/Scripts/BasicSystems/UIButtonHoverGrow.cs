using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHoverGrow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Config")]
    public float scaleMultiplier = 1.1f; // 1.1 = cresce 10%
    public float speed = 10f;            // velocidade da animação

    private Vector3 originalScale;
    private Vector3 targetScale;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        // 🔥 Usa unscaledDeltaTime para funcionar com jogo pausado
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.unscaledDeltaTime * speed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * scaleMultiplier;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }
}