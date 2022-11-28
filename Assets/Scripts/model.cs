// Actividad Integradora 1
// Mayra Fernanda Camacho Rodríguez	A01378998
// Víctor Martínez Román			A01746361
// Melissa Aurora Fadanelli Ordaz		A01749483
// Juan Pablo Castañeda Serrano		A01752030
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class AgentData
{
    public string id;
    public float x, y, z;

    public AgentData(string id, float x, float y, float z)
    {
        this.id = id;
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

[Serializable]
public class AgentsData
{
    public List<AgentData> positions;

    public AgentsData() => this.positions = new List<AgentData>();
}

[Serializable]
public class LightData
{
    public string id;
    public float x, y, z;
    public bool state;
    public bool rotate;

    public LightData(string id, float x, float y, float z, bool state, bool rotate)
    {
        this.id = id;
        this.x = x;
        this.y = y;
        this.z = z;
        this.state = state;
        this.rotate = rotate;
    }
}

[Serializable]
public class LightsData
{
    public List<LightData> positions;

    public LightsData() => this.positions = new List<LightData>();
}

public class model : MonoBehaviour
{
    string serverUrl = "http://localhost:8585";
    string getCars = "/getCars";
    string getLights = "/getLights";
    string getShelves = "/getShelves";
    string sendConfig = "/init";
    string update = "/update";
    AgentsData carsData, shelvesData;
    LightsData lightData;
    Dictionary<string, GameObject> cars;
    Dictionary<string, GameObject> lights;
    Dictionary<string, GameObject> shelves;
    Dictionary<string, Vector3> prevPositions, currPositions;
    Dictionary<string, Vector3> pPosLight, cPosLight;
    Dictionary<string, Vector3> pPosSh, cPosSh;

    bool updated, started, upLight, stLight = false;

    public GameObject carPrefab, lightPrefab;
    public int box;
    public float timeToUpdate = 5.0f;
    private float timer, dt;

    void Start()
    {
        carsData = new AgentsData();
        lightData = new LightsData();
        shelvesData = new AgentsData();

        prevPositions = new Dictionary<string, Vector3>();
        currPositions = new Dictionary<string, Vector3>();
        pPosLight = new Dictionary<string, Vector3>();
        cPosLight = new Dictionary<string, Vector3>();
        pPosSh = new Dictionary<string, Vector3>();
        cPosSh = new Dictionary<string, Vector3>();

        cars = new Dictionary<string, GameObject>();
        lights = new Dictionary<string, GameObject>();
        shelves = new Dictionary<string, GameObject>();

        //floor.transform.localScale = new Vector3((float)width/10, 1, (float)height/10);
        //floor.transform.localPosition = new Vector3((float)width/2-0.5f, 0, (float)height/2-0.5f);
        
        timer = timeToUpdate;

        StartCoroutine(SendConfiguration());
    }

    private void Update() 
    {
        if(timer < 0)
        {
            timer = timeToUpdate;
            updated = false;
            StartCoroutine(UpdateSimulation());
        }

        if (updated)
        {
            timer -= Time.deltaTime;
            dt = 1.0f - (timer / timeToUpdate);

            foreach(var car in currPositions)
            {
                Vector3 currentPosition = car.Value;
                Vector3 previousPosition = prevPositions[car.Key];

                Vector3 interpolated = Vector3.Lerp(previousPosition, currentPosition, dt);
                Vector3 direction = currentPosition - interpolated;

                cars[car.Key].transform.localPosition = interpolated;
                if(direction != Vector3.zero) cars[car.Key].transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        //if (upBox)
        //{
        //    foreach(var box in cPosBox)
        //    {
        //        Vector3 currentPositionBox = box.Value;
        //        Vector3 previousPositionBox = pPosBox[box.Key];

        //        Vector3 interpolated = Vector3.Lerp(previousPositionBox, currentPositionBox, dt);
        //        Vector3 direction = currentPositionBox - interpolated;

        //        boxes[box.Key].transform.localPosition = interpolated;
        //        if(direction != Vector3.zero) boxes[box.Key].transform.rotation = Quaternion.LookRotation(direction);
        //    }
        //}
    }
 
    IEnumerator UpdateSimulation()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + update);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            StartCoroutine(GetCarsData());
            StartCoroutine(GetLightData());
        }
    }

    IEnumerator SendConfiguration()
    {
        WWWForm form = new WWWForm();

        form.AddField("box", box.ToString());
        //form.AddField("width", width.ToString());
        //form.AddField("height", height.ToString());

        UnityWebRequest www = UnityWebRequest.Post(serverUrl + sendConfig, form);
        www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            StartCoroutine(GetCarsData());
            StartCoroutine(GetLightData());
            //StartCoroutine(GetShelvesData());
        }
    }

    IEnumerator GetCarsData() 
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getCars);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            carsData = JsonUtility.FromJson<AgentsData>(www.downloadHandler.text);

            foreach(AgentData car in carsData.positions)
            {
                Vector3 newCarPosition = new Vector3(car.x, car.y, car.z);

                    if(!started)
                    {
                        prevPositions[car.id] = newCarPosition;
                        cars[car.id] = Instantiate(carPrefab, newCarPosition, Quaternion.identity);
                    }
                    else
                    {
                        Vector3 currentPosition = new Vector3();
                        if(currPositions.TryGetValue(car.id, out currentPosition))
                            prevPositions[car.id] = currentPosition;
                        currPositions[car.id] = newCarPosition;
                    }
            }

            updated = true;
            if(!started) started = true;
        }
    }

    IEnumerator GetLightData() 
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getLights);
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else 
        {
            lightData = JsonUtility.FromJson<LightsData>(www.downloadHandler.text);

            foreach(LightData light in lightData.positions)
            {
                Vector3 newLightPosition = new Vector3(light.x, 0, light.z);
                  if(!stLight)
                  {
                      pPosLight[light.id] = newLightPosition;
                      if(!light.rotate) {
                          lights[light.id] = Instantiate(lightPrefab, newLightPosition, Quaternion.Euler(0, 90, 0));
                      } else {
                          lights[light.id] = Instantiate(lightPrefab, newLightPosition, Quaternion.identity);
                      }
                  }
                  else
                  {
                      if(light.state) {
                          lights[light.id].GetComponent<Renderer>().materials[0].color = Color.green;
                      } else {
                          lights[light.id].GetComponent<Renderer>().materials[0].color = Color.red;
                      }
                  }
            }

            upLight = true;
            if(!stLight) stLight = true;
        }
    }

    IEnumerator GetShelvesData()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverUrl + getShelves);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else
        {
            shelvesData = JsonUtility.FromJson<AgentsData>(www.downloadHandler.text);

            foreach(AgentData shelve in shelvesData.positions)
            {
                //Instantiate(shelvePrefab, new Vector3(shelve.x, shelve.y, shelve.z), Quaternion.identity);
            }
        }
    }
}
