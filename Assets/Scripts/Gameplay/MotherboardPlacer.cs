using UnityEngine;

public class MotherboardPlacer : MonoBehaviour
{
    [Header("Referências")]
    public Api api;                  // arraste aqui o objeto que tem o Api.cs
    public BoxCollider snapZone;     // arraste a MotherboardSnapZone (BoxCollider com IsTrigger)
    public Transform snapAnchor;     // arraste o SnapAnchor (opcional, mas recomendado)

    [Header("Movimento")]
    public bool useAxisX = true;     // true = mover no eixo X; false = no Z
    public float moveSpeed = 2.0f;   // unidades por segundo
    public float liftY = 5.5f;       // altura quando segurar (Hold)
    public Vector2 moveLimits = new Vector2(-0.5f, 0.5f); // limites de deslocamento no eixo escolhido

    private Vector3 originalPos;
    private Quaternion originalRot;
    private float accum;             // deslocamento acumulado no eixo
    private bool prevHolding;
    private bool isSnapped;

    void Start()
    {
        originalPos = transform.position;
        originalRot = transform.rotation;
        accum = 0f;
    }

    void Update()
    {
        if (isSnapped || api == null) return;

        bool holding = api.IsHolding;
        string side = api.CurrentSide; // "left","center","right"

        // Direção: left=-1, right=+1, center=0
        float dir = side == "right" ? +1f : side == "left" ? -1f : 0f;

        // Integra por tempo p/ velocidade consistente
        accum += dir * moveSpeed * Time.deltaTime;

        // Se center, volta suavemente pro zero
        if (side == "center")
            accum = Mathf.Lerp(accum, 0f, Time.deltaTime * 5f);

        // Limites
        accum = Mathf.Clamp(accum, moveLimits.x, moveLimits.y);

        // Monta posição alvo
        Vector3 target = originalPos;
        if (useAxisX) target.x = originalPos.x + accum;
        else          target.z = originalPos.z + accum;
        target.y = holding ? liftY : originalPos.y;

        // Transição Hold -> Free: tentativa de snap se estiver dentro da zona
        if (prevHolding && !holding && IsInsideZone(transform.position))
        {
            DoSnap();
            return;
        }

        // Suaviza posição/rotação enquanto não snapado
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 5f);
        Quaternion targetRot = holding
            ? Quaternion.Euler(-90f, originalRot.eulerAngles.y, originalRot.eulerAngles.z)
            : originalRot;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 5f);

        prevHolding = holding;
    }

    bool IsInsideZone(Vector3 worldPos)
    {
        if (snapZone == null) return false;
        Vector3 local = snapZone.transform.InverseTransformPoint(worldPos);
        Vector3 half  = snapZone.size * 0.5f;
        return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
    }

    void DoSnap()
    {
        if (snapAnchor != null)
            transform.SetPositionAndRotation(snapAnchor.position, snapAnchor.rotation);
        else
        {
            // Fallback: deita mantendo X/Z atuais
            Vector3 p = transform.position;
            transform.SetPositionAndRotation(new Vector3(p.x, originalPos.y, p.z),
                Quaternion.Euler(-90f, originalRot.eulerAngles.y, originalRot.eulerAngles.z));
        }

        isSnapped = true;
        Debug.Log("Motherboard snapped.");
    }

    void OnDrawGizmosSelected()
    {
        // Gizmo do eixo de movimento
        Gizmos.color = Color.cyan;
        Vector3 a = transform.position + (useAxisX ? Vector3.right : Vector3.forward) * moveLimits.x;
        Vector3 b = transform.position + (useAxisX ? Vector3.right : Vector3.forward) * moveLimits.y;
        Gizmos.DrawLine(a, b);
    }
}
