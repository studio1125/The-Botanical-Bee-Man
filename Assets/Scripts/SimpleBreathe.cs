using System.Collections;
using UnityEngine;

public class SimpleBreathe : MonoBehaviour {

    [Header("Customization")]
    [SerializeField][Min(1)] float sizeMult;
    [SerializeField][Min(0)] float time;
    private float startRot;
    private bool growing;
    private bool readyToGo;

    Vector3 defaultScale;

    private void Start() {

        growing = true;
        readyToGo = true;
        defaultScale = transform.localScale;

    }

    private void Update() {

        if (readyToGo) {

            if (growing)
                StartCoroutine(EaseLerpTo(defaultScale, defaultScale * sizeMult));
            else
                StartCoroutine(EaseLerpTo(defaultScale * sizeMult, defaultScale));

            growing = !growing;

        }
    }


    private IEnumerator EaseLerpTo(Vector3 startSize, Vector3 endSize) {

        readyToGo = false;
        float elapsed = 0;
        float time; //smoothening formula

        while (elapsed < this.time) {

            time = elapsed / this.time;
            time = time * time * (3 - 2 * time);
            transform.localScale = Vector3.Lerp(startSize, endSize, time);
            elapsed += Time.deltaTime;
            yield return null;

        }

        transform.localScale = endSize;
        readyToGo = true;

    }
}
