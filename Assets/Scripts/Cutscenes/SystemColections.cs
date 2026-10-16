using System.Collections;

using UnityEngine;

using GDavini;

public class FaderCreator : MonoBehaviour

{

    public bool fadeOutOnStart = true;

    public float fadeDelay = 0.5f;

    public float fadeDuration = 3f;

    public GameObject _blackCurtains;

    private void Awake()

    {

        if (_blackCurtains) _blackCurtains.SetActive(true);

    }

    private void Start() => StartCoroutine(LateStart());

    private IEnumerator LateStart()

    {

        yield return null;

        Fader.ClearListeners();

        //Debug.Log("executing fade out");

        if (fadeOutOnStart) FadeOut(fadeDuration);

        if (_blackCurtains) _blackCurtains.SetActive(false);

    }

    public void FadeIn(float _duration) => Fader.FadeIn(fadeDelay, _duration);

    public void FadeOut(float _duration) => Fader.FadeOut(fadeDelay, _duration);

    public void FadeInUnscaled(float _duration) => Fader.FadeIn(fadeDelay, _duration, true);

    public void FadeOutUnscaled(float _duration) => Fader.FadeOut(fadeDelay, _duration, true);

}