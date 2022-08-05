using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Xml.Serialization;
using TMPro;
using Unity.VisualScripting;

public class GameManager : StaticInstance<GameManager>{
    public static event System.Action<GameState> OnBeforeStateChanged;
    public static event System.Action<GameState> OnAfterStateChanged;
    public GameState State { get; private set; }

    [SerializeField] private Transform _dicePrefab;
    [SerializeField] private GameObject _countdownTimer;

    //UI Screens
    [SerializeField] private GameObject _ScreenMenu;
    [SerializeField] private GameObject _ScreenWin;
    [SerializeField] private GameObject _ScreenLose;

    [SerializeField] private List<Sprite> _dotSprites;
    [SerializeField] private List<Sprite> _NumberSprites;
    [SerializeField] private List<Sprite> _GreekSprites;
    [SerializeField] private List<Sprite> _DiceShapeSprites;

    private List<Color> _cores = new List<Color>();
    

    [SerializeField] private float sizeOfTheMap = 15;
    [SerializeField] private int _placeMentAtempt = 30;
    [SerializeField] private float _diceDistance = 3;
    [SerializeField] private int _rotationRange = 0;
    [SerializeField] private float _cameraScaling = 0.85f;

    private Camera _myCamera;

    public GameObject _findText;
    public GameObject _LevelText;
    public GameObject _UiDice;

    [SerializeField] private Animator _anim;


    private List<Vector2> _points;
    [SerializeField] private List<Transform> _dados = new List<Transform>();
    [SerializeField] private List<Transform> _dadosCorretos = new List<Transform>();
    public List<Transform> _dadosClicados = new List<Transform>();
    private DiceType Special;

    //!!
    public int _dificuldade = 0;
    public int _level = 0;
    public int _time = 0;
    public int _nunberOfDice = 3;

    public List<AudioClip> _audioClips = new List<AudioClip>();

    void Start(){
        // set values for variables
        _myCamera = Camera.main;
        _cores.Add(Color.gray);
        _cores.Add(Color.white);
        _cores.Add(Color.red);
        _cores.Add(Color.blue);
        _cores.Add(Color.green);
        _cores.Add(Color.magenta);
        _cores.Add(Color.yellow);
        _cores.Add(Color.cyan);

        // Diactivate Colision on UI Dice
        _UiDice.transform.GetComponent<BoxCollider2D>().enabled = false;

        ChangeState(GameState.Start);     
    }

    public void ChangeState(GameState newState) {
        OnBeforeStateChanged?.Invoke(newState);

        State = newState;
        switch (newState) {
            case GameState.Start:
                _dificuldade = 0;
                sizeOfTheMap = 10;
                _nunberOfDice = 3;
                _rotationRange = 0;
                _level = 0;

                AudioSystem.Instance.PlaySound(_audioClips[0]);
                _ScreenWin.SetActive(false);
                _ScreenLose.SetActive(false);
                _ScreenMenu.SetActive(true);
                break;
            case GameState.Generation:
                DestroyDice();
                GenerateLevel();
                ChangeState(GameState.Play);
                break;
            case GameState.Play:
                break;
            case GameState.Transition:
                break;
            case GameState.Win:
                AudioSystem.Instance.PlaySound(_audioClips[0]);
                _ScreenWin.SetActive(true);
                break;
            case GameState.Lose:
                AudioSystem.Instance.PlaySound(_audioClips[5]);
                _ScreenLose.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnAfterStateChanged?.Invoke(newState);
        
        Debug.Log($"New state: {newState}");
    }

    public void StartButton(){
        _ScreenMenu.SetActive(false);
        _ScreenWin.SetActive(false);
        _ScreenLose.SetActive(false);

        DestroyDice();
        _dificuldade = 0;
        sizeOfTheMap = 10;
        _nunberOfDice = 3;
        _rotationRange = 0;
        _level = 0;

        ChangeState(GameState.Generation);
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.R)){
            DestroyDice();
            ChangeState(GameState.Start);
        }

        if(State == GameState.Play){
            HandleTimer();

            if(CompareLists(_dadosClicados, _dadosCorretos)) {
                DestroyDice();
                GenerateLevel();
            }

            if(_level >= 100){
                DestroyDice();
                ChangeState(GameState.Win);
            }

            if(currentTime <= 0f){
                DestroyDice();
                ChangeState(GameState.Lose);
            }

            if(Input.GetKeyDown(KeyCode.E)){
                DestroyDice();
                GenerateLevel();
            }
        }   
    }

    private float currentTime = 0f;
    private float startingTime = 10f;
    [SerializeField] GameObject _countdownText;

    void HandleTimer(){
        currentTime -= 1* Time.deltaTime;
        _countdownText.GetComponent<TextMeshProUGUI>().text = currentTime.ToString("0.00");
        if(currentTime <= 0) currentTime = 0;
    }

    public void QuitGame(){
        Debug.Log("GameClosed");
        Application.Quit();
    }

    private void GenerateLevel(){
        //Handle Level UP
        GenerateDice(sizeOfTheMap*_cameraScaling, sizeOfTheMap, _nunberOfDice, _dificuldade);

        //PlaySounds
        AudioSystem.Instance.PlaySound(_audioClips[1]); // 15
        if(_level >= 10){
            AudioSystem.Instance.PlaySound(_audioClips[2]);
            if(_level >= 15){
                AudioSystem.Instance.PlaySound(_audioClips[3]);
                if(_level >= 25){
                    AudioSystem.Instance.PlaySound(_audioClips[4]);
                } 
            }
        }
        
        AudioSystem.Instance.PlaySound(_audioClips[1]);

        //Set Up Timer
        currentTime = startingTime;
        _countdownText.GetComponent<TextMeshProUGUI>().text = currentTime.ToString("0.00");

        _level++;
        if(_level <=30){
            if((_level % 3) == 0) _dificuldade++;
            if((_level % 5) == 0) sizeOfTheMap += 5;
            
        }else if(_level <=100){
            if((_level % 5) == 0) startingTime -= 1f;
            if((_level % 15) == 0) sizeOfTheMap += 5;
        }

        if(_level == 20) _cameraScaling = 0.7f;

        if(_level == 9) startingTime = 15f;
        if(_level == 18) startingTime = 30f;

        if(_level == 20) _nunberOfDice = 5;
        if(_level == 10) _rotationRange = 360;
    }

    private void DecorateDice(Transform dice, Color cor, int valor, int type, int shape){

        //set Color
        if(cor == Color.white || cor == Color.cyan){
            dice.GetComponent<SpriteRenderer>().color = cor;
            dice.Find("ValueSprite").GetComponent<SpriteRenderer>().color = Color.black;
        }else{
            dice.GetComponent<SpriteRenderer>().color = cor;
            dice.Find("ValueSprite").GetComponent<SpriteRenderer>().color = Color.white;
        }

        switch (type){
            case 1:
                dice.Find("ValueSprite").GetComponent<SpriteRenderer>().sprite = _NumberSprites[valor];
                break;
            case 2:
                dice.Find("ValueSprite").GetComponent<SpriteRenderer>().sprite = _GreekSprites[valor];
                break;
            default:
                dice.Find("ValueSprite").GetComponent<SpriteRenderer>().sprite = _dotSprites[valor];
                break;
        }

        dice.GetComponent<SpriteRenderer>().sprite = _DiceShapeSprites[shape];
    }

    private void GenerateDice(float cameraSize, float mapSize, int numberOfDice = 0, int dificulty = 0){

        List<Vector2> points = PoissonDiscSampling.GeneratePoints(_diceDistance, new Vector2((mapSize/9)*16, mapSize*1.2f), _placeMentAtempt);

        float cameraPositionx = mapSize - (cameraSize)/3;
        float cameraPositiony = mapSize + (cameraSize)/3;
        _myCamera.transform.position = new Vector3(((cameraPositionx)/9)*8, (cameraPositiony)/2, -10);
        _myCamera.orthographicSize = cameraSize;

        if (points != null) {
            //Instatiate Dice
            foreach (Vector2 point in points) {
                float rotation = UnityEngine.Random.Range(0, _rotationRange);
                Transform NewDado = Instantiate(_dicePrefab, point, Quaternion.Euler(0,0, rotation));
                _dados.Add(NewDado);
            }

            int SpecialDice = 0;
            Helpers.Shuffle<Transform>(_dados);

            DiceType specialStl = GenerateSpecial(dificulty);

            _findText.GetComponent<TextMeshProUGUI>().text = _nunberOfDice.ToString() + " Of ";
            _LevelText.GetComponent<TextMeshProUGUI>().text = "Level: " + _level.ToString();
            DecorateDice(_UiDice.transform, specialStl.cor, specialStl.valor, specialStl.tipo, specialStl.forma);

            //!test
            Debug.Log(specialStl.cor);
            Debug.Log(specialStl.valor);
            Debug.Log(specialStl.tipo);
            Debug.Log(specialStl.forma);

            //Decorate Dice
            foreach (Transform dado in _dados) {
                //?Debug.Log(valorTeste);
                if(SpecialDice < numberOfDice){       
                    DecorateDice(dado, specialStl.cor, specialStl.valor, specialStl.tipo, specialStl.forma);
                    _dadosCorretos.Add(dado);
                }else{
                    DiceType newStl = GenerateSpecial(dificulty);
                    while((newStl.cor == specialStl.cor) && (newStl.valor == specialStl.valor) && (newStl.tipo == specialStl.tipo) && (newStl.forma == specialStl.forma)){
                        newStl = GenerateSpecial(dificulty);
                    }

                    DecorateDice(dado, newStl.cor, newStl.valor, newStl.tipo, newStl.forma);
                }
                SpecialDice++;
            }
        }
    }

    private DiceType GenerateSpecial(int dificulty){
        DiceType retorno = new DiceType();
        retorno.valor = UnityEngine.Random.Range(0, 6);

        switch (dificulty){
            case 0:
                retorno.cor = Color.white; // cor do dado
                retorno.tipo = 0; // tipo do dado
                retorno.forma = 0; // forma do dado
                break;
            case 1:
                retorno.cor = Color.white; // cor do dado
                retorno.tipo = UnityEngine.Random.Range(0, 2); // tipo do dado
                retorno.forma = 0; // forma do dado
                break;
            case 2:
                retorno.cor = _cores[UnityEngine.Random.Range(0, 8)]; // cor do dado
                retorno.tipo = 0; // tipo do dado
                retorno.forma = 0; // forma do dado
                break;
            case 3:
                retorno.cor = _cores[UnityEngine.Random.Range(0, 8)]; // cor do dado
                retorno.tipo = UnityEngine.Random.Range(0, 2); // tipo do dado
                retorno.forma = 0; // forma do dado
                break;
            case 4:
                retorno.cor = Color.white; // cor do dado
                retorno.tipo = UnityEngine.Random.Range(0, 3); // tipo do dado
                retorno.forma = 0; // forma do dado
                break;
            case 5:
                retorno.cor = _cores[UnityEngine.Random.Range(0, 8)]; // cor do dado
                retorno.tipo = UnityEngine.Random.Range(0, 3); // tipo do dado
                retorno.forma = 0; // forma do dado
                break;
            case 6:
                retorno.cor = Color.white; // cor do dado
                retorno.tipo = UnityEngine.Random.Range(0, 2); // tipo do dado
                retorno.forma = UnityEngine.Random.Range(0, 3); // forma do dado
                break;
            case 7:
                retorno.cor = _cores[UnityEngine.Random.Range(0, 8)]; // cor do dado
                retorno.tipo = 0; // tipo do dado
                retorno.forma = UnityEngine.Random.Range(0, 3); // forma do dado
                break;
            default:
                retorno.cor = _cores[UnityEngine.Random.Range(0, 8)]; // cor do dado
                retorno.tipo = UnityEngine.Random.Range(0, 3); // tipo do dado
                retorno.forma = UnityEngine.Random.Range(0, 3); // forma do dado
                break;
        }
        return retorno;
    }

    private void DestroyDice(){
        foreach (var dado in _dados){
            Destroy(dado.gameObject);
        }

        _dados = new List<Transform>();
        _dadosCorretos = new List<Transform>();
        _dadosClicados = new List<Transform>();
    } 

    private bool CompareLists<T>(List<T> aListA, List<T> aListB){
        if (aListA == null || aListB == null || aListA.Count != aListB.Count)
            return false;
        if (aListA.Count == 0)
            return true;
        Dictionary<T, int> lookUp = new Dictionary<T, int>();
        // create index for the first list
        for(int i = 0; i < aListA.Count; i++)
        {
            int count = 0;
            if (!lookUp.TryGetValue(aListA[i], out count))
            {
                lookUp.Add(aListA[i], 1);
                continue;
            }
            lookUp[aListA[i]] = count + 1;
        }
        for (int i = 0; i < aListB.Count; i++)
        {
            int count = 0;
            if (!lookUp.TryGetValue(aListB[i], out count))
            {
                // early exit as the current value in B doesn't exist in the lookUp (and not in ListA)
                return false;
            }
            count--;
            if (count <= 0)
                lookUp.Remove(aListB[i]);
            else
                lookUp[aListB[i]] = count;
        }
        // if there are remaining elements in the lookUp, that means ListA contains elements that do not exist in ListB
        return lookUp.Count == 0;
    }
}

[Serializable]
public struct DiceType{
    public int valor;
    public Color cor;
    public int tipo; //0 = dots; 1 = Nunbers; 2 = Greek;
    public int forma; //0 = Normal; 1 = Rounded; 2 = Greek;
}

public enum GameState {
    Start = 0,
    Generation = 1,
    Play = 2,
    Transition = 3,
    Win = 4,
    Lose = 5,
}