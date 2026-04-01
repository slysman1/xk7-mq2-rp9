// File: TransformAnimator.cs


using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Alexdev
{
    public static class TweenUtils
    {

        public static IEnumerator MoveUI(RectTransform rect, Vector2 targetAnchoredPos, float duration)
        {
            Vector2 startPos = rect.anchoredPosition;
            float elapsed = 0f;

            // If already at target, skip interpolation
            if ((startPos - targetAnchoredPos).sqrMagnitude < 0.0001f)
                yield break;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                rect.anchoredPosition = Vector2.Lerp(startPos, targetAnchoredPos, t);
                yield return null;
            }

            rect.anchoredPosition = targetAnchoredPos;
        }

        public static IEnumerator ScaleUI(RectTransform rect, Vector3 targetScale, float duration)
        {
            Vector3 startScale = rect.localScale;

            if (duration <= 0f)
            {
                rect.localScale = targetScale;
                yield break;
            }

            float elapsed = 0f;

            // Skip if already at target
            if ((startScale - targetScale).sqrMagnitude < 0.0001f)
                yield break;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                rect.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            rect.localScale = targetScale;
        }



        public static IEnumerator ArcLocal(Transform t, Vector3 toLocal, float arcHeight, float duration)
        {
            Vector3 fromLocal = t.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float p = elapsed / duration;

                Vector3 flat = Vector3.Lerp(fromLocal, toLocal, p);

                float curve = Mathf.Sin(p * Mathf.PI);
                flat.y += curve * curve * arcHeight;
                t.localPosition = flat;
                elapsed += Time.deltaTime;
                yield return null;
            }

            t.localPosition = toLocal;
        }
        public static IEnumerator ArcMovement(Transform transform, Transform target, Vector3 targetOffset, float arcHeight, float duration)
        {
            Vector3 start = transform.position;
            float time = 0f;

            while (time < duration)
            {
                float t = time / duration;
                Vector3 flatPos = Vector3.Lerp(start, target.position + targetOffset, t);
                float curve = Mathf.Sin(t * Mathf.PI) * Mathf.Sin(t * Mathf.PI); // smooth arc
                flatPos.y += curve * arcHeight;

                transform.position = flatPos;
                time += Time.deltaTime;
                yield return null;
            }

            transform.position = target.position + targetOffset;
        }
        public static IEnumerator ArcMoveToVector(Transform transform, Vector3 target, float arcHeight, float duration)
        {
            Vector3 start = transform.position;
            float time = 0f;

            while (time < duration)
            {
                float t = time / duration;
                Vector3 flatPos = Vector3.Lerp(start, target, t);
                float curve = Mathf.Sin(t * Mathf.PI) * Mathf.Sin(t * Mathf.PI); // smooth arc
                flatPos.y += curve * arcHeight;

                transform.position = flatPos;
                time += Time.deltaTime;
                yield return null;
            }

            transform.position = target;
        }
        public static IEnumerator MovePosition(Transform transform, Vector3 target, float duration,float delay = 0)
        {
            yield return new WaitForSeconds(delay);
            Vector3 startPos = transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.position = Vector3.Lerp(startPos, target, t);
                yield return null;
            }

            transform.position = target;

        }
        public static IEnumerator MoveLocalPosition(Transform transform, Vector3 target, float duration)
        {
            Vector3 startPos = transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localPosition = Vector3.Lerp(startPos, target, t);
                yield return null;
            }

            transform.localPosition = target;

        }
        public static IEnumerator SetRotationAs(Transform transform, Vector3 targetEulerAngles, float duration)
        {
            Quaternion originalRot = transform.rotation;
            Quaternion targetRot = Quaternion.Euler(targetEulerAngles);

            // If already at target, wait for consistency
            if (Quaternion.Angle(originalRot, targetRot) < 0.01f)
            {
                yield return new WaitForSeconds(duration);
                yield break;
            }

            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);
                transform.rotation = Quaternion.Lerp(originalRot, targetRot, t);
                yield return null;
            }

            transform.rotation = targetRot;
        }
        public static IEnumerator SetLocalRotationAs(Transform transform, Vector3 targetEulerAngles, float duration)
        {
            Quaternion originalRot = transform.localRotation;
            Quaternion targetRot = Quaternion.Euler(targetEulerAngles);

            // If already at target, wait for consistency
            if (Quaternion.Angle(originalRot, targetRot) < 0.01f)
            {
                yield return new WaitForSeconds(duration);
                yield break;
            }

            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);
                transform.localRotation = Quaternion.Lerp(originalRot, targetRot, t);
                yield return null;
            }

            transform.localRotation = targetRot;
        }
        public static IEnumerator RotateLocal(Transform transform, Vector3 angle, float duration)
        {
            if (angle.magnitude < .001f)
                angle = Vector3.one * 0.001f; // Need it so we can send Vector3.zero

            Quaternion originalRot = transform.localRotation;
            Quaternion targetRot = Quaternion.Euler(
                transform.localEulerAngles.x + angle.x,
                transform.localEulerAngles.y + angle.y,
                transform.localEulerAngles.z + angle.z
            );

            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);
                transform.localRotation = Quaternion.Lerp(originalRot, targetRot, t);
                yield return null;
            }

            transform.localRotation = targetRot;
        }
        public static IEnumerator ScaleLocal(Transform transform, Vector3 targetScale, float duration)
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;

            // Handle zero-or-negative duration defensively
            if (duration <= 0f)
            {
                transform.localScale = targetScale;
                yield break;
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            transform.localScale = targetScale;
        }

        public static IEnumerator RotateTowardsTarget(Transform transform, Transform target, float duration, Vector3 up = default)
        {
            if (up == default) up = Vector3.up;

            Quaternion startRot = transform.rotation;
            Quaternion targetRot = Quaternion.LookRotation(target.position - transform.position, up);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
                yield return null;
            }

            transform.rotation = targetRot;
        }



        public static Vector3 GetWorldVelocity(Vector3 velocity)
        {
            Vector3 worldVelocity =
             Vector3.forward * velocity.z +
             Vector3.right * velocity.x +
             Vector3.up * velocity.y;

            return worldVelocity;
        }

        // ---------- UI Image ----------
        public static IEnumerator ColorTo(Image image, Color target, float duration)
        {
            Color start = image.color;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                image.color = Color.Lerp(start, target, t / duration);
                yield return null;
            }

            image.color = target;
        }

        // ---------- TMP Text ----------
        public static IEnumerator ColorTo(TMP_Text text, Color target, float duration)
        {
            Color start = text.color;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                text.color = Color.Lerp(start, target, t / duration);
                yield return null;
            }

            text.color = target;
        }

        public static IEnumerator SetCanvasAlphaTo(CanvasGroup canvasGroup, float targetAlpha, float fadeDuration)
        {
            float startAlpha = canvasGroup.alpha;
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float t = timer / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
        }
    }
#if UNITY_EDITOR

    public static class DataUtils
    {
        /// <summary>
        /// Collects all ScriptableObject assets of type T from the project.
        /// </summary>
        public static T[] GetData<T>() where T : ScriptableObject
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    assets.Add(asset);
            }

            return assets.ToArray();
        }
    }
#endif
}
