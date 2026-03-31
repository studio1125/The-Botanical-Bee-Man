using System.Collections;
using UnityEngine;

public class SimpleSway : MonoBehaviour {

    [Header("Customization")]
    [SerializeField][Min(0)] float degreeOffset;
    [SerializeField][Min(0)] float time;
    private float startRot;
    private bool goingRight; // imagine an arrow placed on top of the object pointing up
    private bool readyToGo;

    private void Start() {

        startRot = transform.rotation.eulerAngles.z;
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, transform.rotation.y, startRot - degreeOffset));

        goingRight = true;
        readyToGo = true;

    }

    private void Update() {

        if (readyToGo) {

            if (goingRight)
                StartCoroutine(EaseLerpTo(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y,
                                                    startRot + degreeOffset)));
            else
                StartCoroutine(EaseLerpTo(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y,
                                                    startRot - degreeOffset)));

            goingRight = !goingRight;

        }
    }


    private IEnumerator EaseLerpTo(Vector3 targetRot) {

        readyToGo = false;
        float elapsed = 0;
        Vector3 startRotEuler = transform.rotation.eulerAngles;
        Quaternion targetQ = Quaternion.Euler(targetRot);
        Quaternion startQ = Quaternion.Euler(startRotEuler);
        float time; //smoothening formula

        while (elapsed < this.time) {

            time = elapsed / this.time;
            time = time * time * (3 - 2 * time);
            transform.rotation = Quaternion.Lerp(startQ, targetQ, time);
            elapsed += Time.deltaTime;
            yield return null;

        }

        transform.rotation = targetQ;
        readyToGo = true;

    }
}
