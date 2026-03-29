using UnityEngine;

public class FunFactDatabase : MonoBehaviour {

    [Header("Data")]
    [SerializeField] private string[] funFacts;

    public string GetRandomFunFact() => funFacts[Random.Range(0, funFacts.Length)];

}
