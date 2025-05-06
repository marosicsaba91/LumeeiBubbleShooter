// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace BubbleShooterKit
{
    /// <summary>
    /// This class is responsible for managing the transitions between scenes
    /// that are performed in the game via a classic fade to/from black.
    /// </summary>
    public class Transition : MonoBehaviour
    {
        private static GameObject canvasObject;

        private GameObject overlay;

        private void Awake()
        {
            canvasObject = new GameObject("TransitionCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            DontDestroyOnLoad(canvasObject);
        }

        public static void LoadLevel(string level, float duration, Color fadeColor)
        {
            GameObject fade = new("Transition");
            fade.AddComponent<Transition>();
            fade.GetComponent<Transition>().StartFade(level, duration, fadeColor);
            fade.transform.SetParent(canvasObject.transform, false);
            fade.transform.SetAsLastSibling();
        }

        private void StartFade(string level, float duration, Color fadeColor)
        {
            StartCoroutine(RunFade(level, duration, fadeColor));
        }

        private IEnumerator RunFade(string level, float duration, Color fadeColor)
        {
            Texture2D bgTex = new(1, 1);
            bgTex.SetPixel(0, 0, fadeColor);
            bgTex.Apply();

            overlay = new GameObject();
            Image image = overlay.AddComponent<Image>();
            Rect rect = new(0, 0, bgTex.width, bgTex.height);
            Sprite sprite = Sprite.Create(bgTex, rect, new Vector2(0.5f, 0.5f), 1);
            image.material.mainTexture = bgTex;
            image.sprite = sprite;
            Color newColor = image.color;
            image.color = newColor;
            image.canvasRenderer.SetAlpha(0.0f);

            overlay.transform.localScale = new Vector3(1, 1, 1);
            overlay.GetComponent<RectTransform>().sizeDelta = canvasObject.GetComponent<RectTransform>().sizeDelta;
            overlay.transform.SetParent(canvasObject.transform, false);
            overlay.transform.SetAsFirstSibling();

            float time = 0.0f;
            float halfDuration = duration / 2.0f;
            while (time < halfDuration)
            {
                time += Time.deltaTime;
                image.canvasRenderer.SetAlpha(Mathf.InverseLerp(0, 1, time / halfDuration));
                yield return new WaitForEndOfFrame();
            }

            image.canvasRenderer.SetAlpha(1.0f);
            yield return new WaitForEndOfFrame();

            SceneManager.LoadScene(level);

            time = 0.0f;
            while (time < halfDuration)
            {
                time += Time.deltaTime;
                image.canvasRenderer.SetAlpha(Mathf.InverseLerp(1, 0, time / halfDuration));
                yield return new WaitForEndOfFrame();
            }

            image.canvasRenderer.SetAlpha(0.0f);
            yield return new WaitForEndOfFrame();

            Destroy(canvasObject);
        }
    }
}
