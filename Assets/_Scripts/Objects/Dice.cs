using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.VisualScripting;

public class Dice : MonoBehaviour{
    [HideInInspector] public bool selecionado = false;
    [HideInInspector] public bool Valid = false;
    private SpriteRenderer _border;

    private GameManager _gameManager;


    private void Start(){
        _border = transform.Find("ClickedSprite").GetComponent<SpriteRenderer>();
        _gameManager = GameManager.Instance;
    }

    void Update(){
        
    }

    private void OnMouseDown() {

        if(_border.enabled){
            _gameManager._dadosClicados.Remove(transform);
        }else{
            _gameManager._dadosClicados.Add(transform);
        }

        _border.enabled = !_border.enabled;   
    }

    private void ClickOnDice(){

    } 
}
