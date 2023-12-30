using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class MapEditor : MonoBehaviour
{
    // zamiast tych to zczyta� to z obiektu environment na mapie
    public static List<GameObject> envObjects;
    public static bool isMenu = false;
    public GameObject optionsMenu;
    public GameObject loginCredentials;
    public GameObject MapPrefab;
    public SpriteRenderer TerrainRenderer;
    
    public TMP_InputField playerMoney;
    public TMP_InputField mapName;
    public TMP_InputField mapWidth;
    public TMP_InputField mapHeight;
    public TMP_InputField playerName;
    public TMP_InputField playerPassword;
    public TMP_Dropdown terrainDropdown;
    public TMP_Dropdown objectDropdown;

    void Start()
    {
        var obj = Instantiate(MapPrefab, Vector3.zero, Quaternion.identity);
        TerrainRenderer = obj.transform.Find("TerrainSprite").GetComponent<SpriteRenderer>();
        if(MapPreview.ChosenMapData != null)
        {
            var mapData = MapPreview.ChosenMapData;
            mapName.text = mapData.name;
            mapWidth.text = mapData.size.x.ToString();
            mapHeight.text = mapData.size.y.ToString();
            playerMoney.text = mapData.playerStartingMoney.ToString();
        } else
        {
            playerMoney.text = "500";
            mapWidth.text = TerrainRenderer.size.x.ToString();
            mapHeight.text = TerrainRenderer.size.y.ToString();
        }
        envObjects = new();
        LoadTerrainOptions();
        LoadObjectOptions();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) ToggleOptionsMenu();
    }

    public void LoadTerrainOptions()
    {
        List<TMP_Dropdown.OptionData> options = new();
        foreach (var key in TerrainDictionary.Sprites.Keys)
        {
            options.Add(new TMP_Dropdown.OptionData(key.ToString()));
        }
        terrainDropdown.value = 0;
        terrainDropdown.AddOptions(options);
    }

    public void LoadObjectOptions()
    {
        List<TMP_Dropdown.OptionData> options = new();
        options.Add(new TMP_Dropdown.OptionData("None"));
        foreach (var key in EnvDictionary.Objects.Keys)
        {
            options.Add(new TMP_Dropdown.OptionData(key.ToString()));
        }
        objectDropdown.value = 0;
        objectDropdown.AddOptions(options);
    }


    public void ChangeTerrain()
    {
        Terrain terrainType;
        Enum.TryParse(terrainDropdown.options[terrainDropdown.value].text, out terrainType);
        Debug.Log(terrainType);
        TerrainRenderer.sprite = TerrainDictionary.Sprites[terrainType];
    }

    public void SelectObject()
    {
        if(objectDropdown.value == 0)
        {
            Destroy(ObjectPlacing.heldObject);
            return;
        }
        Env objectType;
        Enum.TryParse(objectDropdown.options[objectDropdown.value].text, out objectType);
        Debug.Log(objectType);
        GameObject obj = Instantiate(EnvDictionary.Objects[objectType], GameObject.FindGameObjectWithTag("mapEnv").transform);
        try
        {
            obj.GetComponent<BoxCollider2D>().enabled = false;
        } catch
        {
            obj.GetComponent<CircleCollider2D>().enabled = false;
        }
        ObjectPlacing.envObjectType = objectType;
        Destroy(ObjectPlacing.heldObject);
        ObjectPlacing.heldObject = obj;
    }

    public void ToggleOptionsMenu()
    {
        if (optionsMenu.activeInHierarchy)
        {
            isMenu = false;
            optionsMenu.SetActive(false);
        } else
        {
            isMenu = true;
            optionsMenu.SetActive(true);
        }
    }

    public void SaveMap()
    {
        if (NetworkManager.username == "")
        {
            ShowLoginFields();
            return;
        };
        if (mapName.text.Length < 3) return;
        var shapeController = GameObject.FindGameObjectWithTag("path").GetComponent<SpriteShapeController>().spline;
        var data = new MapData();
        int pointCount = shapeController.GetPointCount();
        data.name = mapName.text;
        data.mapAuthor = NetworkManager.username;
        data.SplinePos = new Vector2[pointCount];
        data.TangentPos = new Vector2[pointCount * 2];
        data.terrainType = (Terrain)Enum.Parse(typeof(Terrain), terrainDropdown.options[terrainDropdown.value].text);
        data.size = TerrainRenderer.size;
        data.EnvObjectsPos = new Vector2[envObjects.Count];
        data.EnvObjectsType = new Env[envObjects.Count];

        //pathPoints
        for (int i = 0; i < pointCount; i++)
        {
            data.SplinePos[i] = shapeController.GetPosition(i);
            data.TangentPos[2 * i] = shapeController.GetLeftTangent(i);
            data.TangentPos[2 * i + 1] = shapeController.GetRightTangent(i);
            Debug.Log(i + " " + shapeController.GetPosition(i));
        }

        //envObjects
        for (int i = 0; i < envObjects.Count; i++)
        {
            data.EnvObjectsPos[i] = envObjects[i].transform.position;
            data.EnvObjectsType[i] = envObjects[i].GetComponent<ObjectHandler>().objectType;
        }

        FileManager.SaveMapData(data);
    }
    public void GoToMenu()
    {
        SoundManager.Instance.PlayButtonClick();
        SceneManager.LoadScene("menu");
    }

    public void SetMapSize()
    {
        int width, height;
        if (!int.TryParse(mapWidth.text, out width)
            || !int.TryParse(mapHeight.text, out height)) return;

        Math.Clamp(width, 100, 1200);
        Math.Clamp(height, 100, 1200);
        mapWidth.text = width.ToString();
        mapHeight.text = height.ToString();
        TerrainRenderer.size = new Vector2(width,height);
    }

    public void UploadMap()
    {
        //check if username and password are set
        //set the mapAuthor to the mapData
    }

    public void ShowLoginFields()
    {
        loginCredentials.SetActive(true);
    }
}