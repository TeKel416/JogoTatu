using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GDavini
{
    /// <summary>
    /// Handles screen fading using a UI <see cref="Image"/> component.
    /// Provides a global singleton interface for triggering fades.
    /// </summary>
    /// <remarks>
    /// This component operates as a singleton and should exist only once in the scene.
    /// It supports:
    /// <list type="bullet">
    /// <item><description>Alpha fades (fade in, fade out, flash)</description></item>
    /// <item><description>Custom color fades</description></item>
    /// <item><description>Time-scale independent fading</description></item>
    /// <item><description>Optional persistence between scene loads</description></item>
    /// </list>
    /// 
    /// <para>
    /// A fade operation overrides any currently running fade.
    /// </para>
    /// </remarks>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)]
    public class Fader : MonoBehaviour
    {
        #region References

        /// <summary>
        /// Global singleton instance of the <see cref="Fader"/>.
        /// </summary>
        /// <remarks>
        /// If multiple instances exist, the most recently initialized one replaces the previous instance.
        /// </remarks>
        public static Fader Instance { get; private set; }

        [Tooltip("Image used for fading. If left empty the first Image found in children will be used.")]
        [SerializeField] private Image _image;

        [Header("Fade Settings")]

        [Tooltip("Delay before the fade starts (in seconds).")]
        [SerializeField, Min(0f)] private float _delay = 2;

        [Tooltip("Duration of the fade (in seconds).")]
        [SerializeField, Min(0.0001f)] private float _duration = 5;

        [Tooltip("If enabled the fader will persist between scene loads.")]
        [SerializeField] private bool _persistent = true;

        [Space(10)]
        [Tooltip("Events called on Start() (useful to call Fader methods such as 'FadeOut')")]
        [SerializeField] private UnityEvent _onStart = new();

        [Space(10)]
        [Tooltip("Events called when a fade finishes (useful for calls such as 'LoadNextScene').")]
        [SerializeField] private UnityEvent _onEnd = new();

        /// <summary>
        /// Event invoked during <see cref="Start"/>.
        /// </summary>
        /// <remarks>
        /// Useful for triggering fade operations automatically at scene start.
        /// </remarks>
        public UnityEvent OnStart => _onStart;

        /// <summary>
        /// Event invoked when a fade operation completes.
        /// </summary>
        /// <remarks>
        /// Triggered after the fade reaches its final state and disables itself.
        /// </remarks>
        public UnityEvent OnEnd => _onEnd;

        #endregion

        #region Internal Logic

        private Color _startColor, _targetColor, _fadeStartColor;
        private float _delayCounter, _time, _startDuration;
        private bool _unscaled = false, _flash = false;

        private void Awake()
        {
            if (!Application.isPlaying) return;

            if (Instance != null && Instance != this)
            {
                Fader old = Instance;
                Instance = this;
                Destroy(old.gameObject);
            }
            else Instance = this;

            if (_image == null && !TryGetComponent(out _image)) _image = GetComponentInChildren<Image>(true);
            if (_image == null)
            {
                Debug.LogError("Fader could not find an Image component.", this);
                enabled = false;
                return;
            }

            if (_persistent) DontDestroyOnLoad(gameObject);
            _startDuration = _duration;
            _startColor = _image.color;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start() => _onStart?.Invoke();

        private void Update()
        {
            if (_delayCounter > 0f) _delayCounter = Mathf.Max(0, _delayCounter - CurrentDeltaTime);
            else
            {
                float t = Mathf.Clamp01((_time += CurrentDeltaTime) / _duration);
                _image.color = Color.Lerp(_fadeStartColor, _targetColor, t);

                if (t >= 1f)
                {
                    _image.color = _targetColor;

                    if (_flash && _targetColor.a == 1f)
                    {
                        _flash = false;
                        ExecuteAlphaFade(_targetColor, false, 0, _duration * 0.5f, false, _unscaled);
                    }
                    else
                    {
                        SetActive(false);
                        _onEnd?.Invoke();
                    }
                }
            }
        }

        private float CurrentDeltaTime => _unscaled ? Time.unscaledDeltaTime : Time.deltaTime;

        private void SetActive(bool activate) => enabled = activate;

        private void ResetFader(float newDelay = -1, float newDuration = -1, bool newUnscaled = false)
        {
            _delayCounter = Mathf.Max(newDelay < 0 ? _delay : newDelay, 0f);
            _duration = Mathf.Max(newDuration < 0 ? _startDuration : newDuration, 0.0001f);
            _unscaled = newUnscaled;
            _time = 0f;
            SetActive(true);
        }

        private void ExecuteAlphaFade(Color newColor, bool fadeIn, float newDelay, float newDuration, bool newFlash, bool newUnscaled)
        {
            if (enabled) StopFade();

            _flash = newFlash;
            if (newFlash && newDuration < 0) newDuration = _startDuration * 0.5f;

            _fadeStartColor = newColor;
            _fadeStartColor.a = fadeIn ? 0f : 1f;

            _targetColor = newColor;
            _targetColor.a = fadeIn ? 1f : 0f;

            _image.color = _fadeStartColor;

            ResetFader(newDelay, newDuration, newUnscaled);
        }

        private void ExecuteColorFade(Color from, Color to, float newDelay, float newDuration, bool newUnscaled)
        {
            if (enabled) StopFade();

            _flash = false;
            _targetColor = to;
            _fadeStartColor = from;
            _image.color = from;

            ResetFader(newDelay, newDuration, newUnscaled);
        }

        private void Reset() => _image = GetComponentInChildren<Image>(true);

        private void OnValidate()
        {
            if (_image == null) _image = GetComponentInChildren<Image>(true);
        }

        #endregion

        #region Static Access Helper

        /// <summary>
        /// Attempts to retrieve the current <see cref="Fader"/> instance.
        /// </summary>
        /// <param name="f">Outputs the current instance if found; otherwise null.</param>
        /// <returns>True if an instance exists in the scene; otherwise false.</returns>
        /// <remarks>
        /// Logs an error if no instance is available.
        /// </remarks>
        private static bool TryGetInstance(out Fader f)
        {
            f = Instance;
            if (f == null)
            {
                Debug.LogError("No Fader instance found in the scene.");
                return false;
            }
            return true;
        }

        #endregion

        #region Alpha Fades (Static)

        /// <summary>
        /// Fades from transparent to opaque using the default color.
        /// </summary>
        /// <param name="delay">
        /// Delay in seconds before the fade starts.
        /// Use -1 to use the currently configured default delay.
        /// </param>
        /// <param name="duration">
        /// Duration of the fade in seconds.
        /// Use -1 to use the default duration.
        /// </param>
        /// <param name="unscaled">
        /// If true, uses <see cref="Time.unscaledDeltaTime"/> instead of <see cref="Time.deltaTime"/>.
        /// This allows the fade to run while the game is paused.
        /// </param>
        /// <remarks>
        /// Overrides any currently active fade.
        /// </remarks>
        public static void FadeIn(float delay = -1, float duration = -1, bool unscaled = false)
        {
            if (!TryGetInstance(out Fader f)) return;
            f.ExecuteAlphaFade(f._startColor, true, delay, duration, false, unscaled);
        }

        /// <summary>
        /// Fades from opaque to transparent using the default color.
        /// </summary>
        /// <param name="delay">
        /// Delay before the fade starts. Use -1 to keep the existing delay.
        /// </param>
        /// <param name="duration">
        /// Duration of the fade. Use -1 to keep the existing duration.
        /// </param>
        /// <param name="unscaled">
        /// If true, ignores time scale.
        /// </param>
        public static void FadeOut(float delay = -1, float duration = -1, bool unscaled = false)
        {
            if (!TryGetInstance(out Fader f)) return;
            f.ExecuteAlphaFade(f._startColor, false, delay, duration, false, unscaled);
        }

        /// <summary>
        /// Performs a flash effect (fade in followed by fade out).
        /// </summary>
        /// <param name="delay">
        /// Delay before the flash starts. Use -1 to use default delay.
        /// </param>
        /// <param name="duration">
        /// Total duration of the flash.
        /// If -1, the duration is split automatically based on the default duration.
        /// </param>
        /// <param name="unscaled">
        /// If true, ignores time scale.
        /// </param>
        /// <remarks>
        /// The fade-in occupies the first half of the duration,
        /// and the fade-out occupies the second half.
        /// </remarks>
        public static void Flash(float delay = -1, float duration = -1, bool unscaled = false)
        {
            if (!TryGetInstance(out Fader f)) return;
            f.ExecuteAlphaFade(f._startColor, true, delay, duration, true, unscaled);
        }

        #endregion

        #region Color Fades (Static)

        /// <summary>
        /// Fades in using a specified color.
        /// </summary>
        /// <param name="color">The color to fade with.</param>
        /// <param name="delay">Delay before starting the fade. Use -1 for default.</param>
        /// <param name="duration">Duration of the fade. Use -1 for default.</param>
        /// <param name="unscaled">If true, ignores time scale.</param>
        public static void FadeInWithColor(Color color, float delay = -1, float duration = -1, bool unscaled = false)
        {
            if (!TryGetInstance(out Fader f)) return;
            f.ExecuteAlphaFade(color, true, delay, duration, false, unscaled);
        }

        /// <summary>
        /// Fades out using a specified color.
        /// </summary>
        /// <param name="color">The color to fade with.</param>
        /// <param name="delay">Delay before starting the fade. Use -1 for default.</param>
        /// <param name="duration">Duration of the fade. Use -1 for default.</param>
        /// <param name="unscaled">If true, ignores time scale.</param>
        public static void FadeOutWithColor(Color color, float delay = -1, float duration = -1, bool unscaled = false)
        {
            if (!TryGetInstance(out Fader f)) return;
            f.ExecuteAlphaFade(color, false, delay, duration, false, unscaled);
        }

        /// <summary>
        /// Performs a flash using a specified color.
        /// </summary>
        /// <param name="color">Flash color.</param>
        /// <param name="delay">Delay before starting.</param>
        /// <param name="duration">Total duration of the flash.</param>
        /// <param name="unscaled">If true, ignores time scale.</param>
        public static void FlashColor(Color color, float delay = -1, float duration = -1, bool unscaled = false)
        {
            if (!TryGetInstance(out Fader f)) return;
            f.ExecuteAlphaFade(color, true, delay, duration, true, unscaled);
        }

        /// <summary>
        /// Fades between two colors.
        /// </summary>
        /// <param name="from">Starting color.</param>
        /// <param name="to">Target color.</param>
        /// <param name="delay">Delay before starting the fade.</param>
        /// <param name="duration">Duration of the transition.</param>
        /// <param name="unscaled">If true, ignores time scale.</param>
        public static void FadeBetweenColors(Color from, Color to, float delay = -1, float duration = -1, bool unscaled = false)
        {
            if (!TryGetInstance(out Fader f)) return;
            f.ExecuteColorFade(from, to, delay, duration, unscaled);
        }

        #endregion

        #region Overrides (Static)

        /// <summary>
        /// Sets the default delay used for future fades.
        /// </summary>
        /// <param name="value">Delay in seconds. Values below 0 are clamped to 0.</param>
        public static void SetDelay(float value)
        {
            if (!TryGetInstance(out Fader f)) return;
            if (!f.enabled) f._delay = Mathf.Max(value, 0f);
        }

        /// <summary>
        /// Sets the default duration used for future fades.
        /// </summary>
        /// <param name="value">Duration in seconds. Values below 0 are clamped.</param>
        public static void SetDuration(float value)
        {
            if (!TryGetInstance(out Fader f)) return;
            if (!f.enabled)
            {
                f._startDuration = Mathf.Max(value, 0f);
                f._duration = f._startDuration;
            }
        }

        /// <summary>
        /// Sets the base color used by fade operations.
        /// </summary>
        /// <param name="color">The new base color.</param>
        public static void SetColor(Color color)
        {
            if (!TryGetInstance(out Fader f)) return;
            if (!f.enabled)
            {
                f._startColor = color;
                f._targetColor = color;
                f._image.color = color;
            }
        }

        /// <summary>
        /// Stops any active fade.
        /// </summary>
        /// <param name="applyTargetColor">
        /// If true, the target color is immediately applied before stopping.
        /// </param>
        /// <remarks>
        /// This fully resets the fade state and disables the component.
        /// </remarks>
        public static void StopFade(bool applyTargetColor = false)
        {
            if (!TryGetInstance(out Fader f)) return;

            if (applyTargetColor && f._image != null) f._image.color = f._targetColor;

            f._time = 0f;
            f._delayCounter = 0f;
            f._flash = false;
            f._unscaled = false;
            f.enabled = false;
        }

        /// <summary>
        /// Removes all listeners from fade events.
        /// </summary>
        /// <remarks>
        /// Clears both <see cref="OnStart"/> and <see cref="OnEnd"/>.
        /// </remarks>
        public static void ClearListeners()
        {
            if (!TryGetInstance(out Fader f)) return;
            f._onStart.RemoveAllListeners();
            f._onEnd.RemoveAllListeners();
        }

        #endregion
    }
}