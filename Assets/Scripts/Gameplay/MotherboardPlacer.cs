using UnityEngine;

public class MotherboardPlacer : MonoBehaviour
{
    [Header("Referências")]
    public Api api;                  // arraste o objeto que tem o Api.cs
    public BoxCollider snapZone;     // arraste a MotherboardSnapZone (BoxCollider com IsTrigger)
    public Transform snapAnchor;     // posição e rotação final da placa-mãe

    [Header("Movimento")]
    public bool useAxisX = true;     // true = mover no eixo X; false = no Z
    public float moveSpeed = 2.0f;   // velocidade de movimento lateral
    public float liftY = 6.5f;       // altura ao segurar (pode ajustar)
    public Vector2 moveLimits = new Vector2(-0.5f, 0.5f); // limites laterais

    [Header("Configuração do Snap")]
    [Tooltip("Margem de tolerância no plano X/Z para encaixe.")]
    public float snapTolerance = 0.05f;

    private Vector3 originalPos;
    private Quaternion originalRot;
    private float accum;
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
        string side = api.CurrentSide;

        // Movimenta lateralmente apenas enquanto segura
        float dir = holding ? (side == "right" ? -1f : side == "left" ? +1f : 0f) : 0f;
        accum += dir * moveSpeed * Time.deltaTime;

        // Retorna suavemente para o centro se estiver segurando e com a mão no centro
        if (holding && side == "center")
            accum = Mathf.Lerp(accum, 0f, Time.deltaTime * 5f);

        accum = Mathf.Clamp(accum, moveLimits.x, moveLimits.y);

        // Define a posição alvo
        Vector3 target = originalPos;
        if (useAxisX) target.x = originalPos.x + accum;
        else          target.z = originalPos.z + accum;
        target.y = holding ? liftY : originalPos.y;

        // Move suavemente e mantém a rotação original
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 5f);
        transform.rotation = originalRot;

        // Quando soltar, verifica se está alinhado em X/Z com a zona de snap
        if (prevHolding && !holding)
        {
            if (IsAlignedXZ(transform.position))
            {
                DoSnap();
                return;
            }
        }

        prevHolding = holding;
    }

    /// <summary>
    /// Verifica se a placa está dentro da área de snap no plano X/Z (ignora Y)
    /// </summary>
    bool IsAlignedXZ(Vector3 worldPos)
    {
        if (snapZone == null) return false;

        Vector3 local = snapZone.transform.InverseTransformPoint(worldPos);
        Vector3 half = Vector3.Scale(snapZone.size * 0.5f, snapZone.transform.lossyScale);

        bool insideX = Mathf.Abs(local.x) <= (half.x + snapTolerance);
        bool insideZ = Mathf.Abs(local.z) <= (half.z + snapTolerance);

        return insideX && insideZ;
    }

    /// <summary>
    /// Move e rotaciona o objeto até o snapAnchor (ou fallback se não houver)
    /// </summary>
    void DoSnap()
    {
        if (snapAnchor != null)
        {
            transform.SetPositionAndRotation(snapAnchor.position, snapAnchor.rotation);
        }
        else
        {
            // fallback: mantém X/Z e ajusta apenas altura e rotação
            Vector3 p = transform.position;
            transform.SetPositionAndRotation(
                new Vector3(p.x, originalPos.y, p.z),
                Quaternion.Euler(-90f, originalRot.eulerAngles.y, originalRot.eulerAngles.z)
            );
        }

        isSnapped = true;
        Debug.Log("✅ Motherboard encaixada (Snap aplicado).");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 a = transform.position + (useAxisX ? Vector3.right : Vector3.forward) * moveLimits.x;
        Vector3 b = transform.position + (useAxisX ? Vector3.right : Vector3.forward) * moveLimits.y;
        Gizmos.DrawLine(a, b);

        if (snapZone != null)
        {
            Gizmos.color = Color.green;
            Matrix4x4 prev = Gizmos.matrix;
            Gizmos.matrix = snapZone.transform.localToWorldMatrix;

            Vector3 sz = Vector3.Scale(snapZone.size, snapZone.transform.lossyScale);
            Vector3 p0 = new Vector3(-sz.x/2 - snapTolerance, 0f, -sz.z/2 - snapTolerance);
            Vector3 p1 = new Vector3( sz.x/2 + snapTolerance, 0f, -sz.z/2 - snapTolerance);
            Vector3 p2 = new Vector3( sz.x/2 + snapTolerance, 0f,  sz.z/2 + snapTolerance);
            Vector3 p3 = new Vector3(-sz.x/2 - snapTolerance, 0f,  sz.z/2 + snapTolerance);

            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p0);

            Gizmos.matrix = prev;
        }
    }
}
