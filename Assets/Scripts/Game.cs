using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    public Player Player;

    [SerializeField]
    private GameObject unitPrefab;

    [SerializeField]
    private TerrainGrid terrain;

    [HideInInspector]
    public List<Unit> Units = new List<Unit>();

    [HideInInspector]
    public static Game Instance { get; private set; } // static singleton


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.N))
        {
            Vector3 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            newPos.y = 0;

            Unit newUnit = Instantiate(unitPrefab, transform).GetComponent<Unit>();
            newUnit.transform.localPosition = newPos;
            Units.Add(newUnit);
        }
    }
}
