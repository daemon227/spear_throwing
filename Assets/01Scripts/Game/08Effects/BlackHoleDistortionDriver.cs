using UnityEngine;
using UnityEngine.UI;

namespace DongA.Effects
{
    /// <summary>
    /// Gắn vào Image dùng material BlackHoleDistortion.
    /// Image này nên lớn hơn sprite blackhole để distortion tràn ra ngoài.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class BlackHoleDistortionDriver : MonoBehaviour
    {
        // Transform thực sự đang xoay (thường là sprite blackhole),
        // dùng để lấy góc xoay hiện tại drive swirl.
        [SerializeField] private Transform _rotatingTarget;

        private Material _mat;
        private RectTransform _rect;

        private static readonly int PropSwirlAngle = Shader.PropertyToID("_SwirlAngle");
        private static readonly int PropCenter     = Shader.PropertyToID("_Center");

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();

            // Tạo instance riêng, không chia sẻ material
            _mat = Instantiate(GetComponent<Image>().material);
            GetComponent<Image>().material = _mat;
        }

        private void OnEnable()
        {
            UpdateShader();
        }

        private void Update()
        {
            UpdateShader();
        }

        private void UpdateShader()
        {
            if (_mat == null) return;

            // --- Góc xoay -> Swirl angle (radian) ---
            Transform src = _rotatingTarget != null ? _rotatingTarget : transform;
            float zDeg    = src.eulerAngles.z;          // 0-360
            float zRad    = zDeg * Mathf.Deg2Rad;
            _mat.SetFloat(PropSwirlAngle, zRad);

            // --- Tâm trong viewport [0,1] ---
            // Dùng null camera cho Canvas Overlay; truyền canvas camera nếu dùng Screen Space - Camera
            Vector2 screenPos  = RectTransformUtility.WorldToScreenPoint(null, _rect.position);
            Vector2 viewportPos = new(screenPos.x / Screen.width, screenPos.y / Screen.height);
            _mat.SetVector(PropCenter, new Vector4(viewportPos.x, viewportPos.y, 0, 0));
        }

        private void OnDestroy()
        {
            if (_mat != null)
                Destroy(_mat);
        }
    }
}
