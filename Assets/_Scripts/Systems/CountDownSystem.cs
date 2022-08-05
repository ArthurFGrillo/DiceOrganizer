// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public class CountDownSystem : MonoBehaviour{

//     float currentTime = 0f;
//     public float startingTime = 10f;

//     [SerializeField] GameObject _countdownText;

//     void Start(){
//         currentTime = startingTime;
//     }

//     void Update(){
//         currentTime -= 1* Time.deltaTime;
//         _countdownText.GetComponent<TextMeshProUGUI>().text = currentTime.ToString("0.00");
//         if(currentTime <= 0) currentTime = 0;
//     }
// }
