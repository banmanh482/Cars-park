using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Cars : MonoBehaviour
{
    public GameObject ControlUI;
    public GameObject StartUI;
    public GameObject OKButton;
    public GameObject gate1;
    public GameObject gate2;
    public GameObject gate3;
    public GameObject gate4;


    private Transform car;
    private Dictionary<Transform, List<Vector2>> cars;
    private Dictionary<Transform, float> carsPosition;
    private Dictionary<Transform, bool> movingCars;
    private Dictionary<Transform, float> carsSpeed;

    private bool isMoving = false;
    private bool isControlOpen = false;
    private bool isMovingOut = false;
    private bool isOpenStartUI = false;

    // Start is called before the first frame update
    void Start()
    {
        cars = new Dictionary<Transform, List<Vector2>>();
        carsPosition = new Dictionary<Transform, float>();
        movingCars = new Dictionary<Transform, bool>();
        Dropdown dropdown = StartUI.GetComponentsInChildren<Dropdown>()[0];
        carsSpeed = new Dictionary<Transform, float>();
        dropdown.ClearOptions();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            //Debug.Log("Mouse down");
            //hit is object that mouse click on
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            //check if click object is car and control is not opening and not chossing temp
            if (!isControlOpen && !isMoving && hit && hit.transform.gameObject.name.Contains("Car"))
            {
                Debug.Log("Hit " + hit.transform.gameObject.name);
                car = hit.transform;
                Debug.Log("Car: " + car.gameObject.name);
                OpenControl();
            }
            //check if click on empty slot or gate when chosing new space for Out and Temp function
            else if ( isMoving && hit && (hit.transform.gameObject.name.Contains("Slot") || hit.transform.gameObject.name.Contains("Gate")))
            {
                Debug.Log("Hit " + hit.transform.gameObject.name);

                var transform = hit.transform;
                Vector2 position = transform.position;

                var carPositions = cars[car];
                carPositions.Add(position);
                //check if car is moving or just get out from car line
                if (isMovingOut)
                {
                    isMovingOut = false;
                    isMoving = false;
                    ControlUI.SetActive(true);
                    OKButton.SetActive(false);
                    CheckCarIsOut();
                }
                //if hit gate then Temp function end right away
                if (hit.transform.gameObject.name.Contains("Gate"))
                {
                    DoneTemp();
                }
            }
            //for debug only
            else
            {
                //Debug.Log("No hit");
            }
        }

        if(cars.Count > 0)
        {
            UpdateDropDown();
            //when a car have new position to move to and have the right in movingCars then system allow car to move
            foreach (KeyValuePair<Transform, List<Vector2>> item in cars)
            {
                if (movingCars.ContainsKey(item.Key) && movingCars[item.Key])
                {
                    float step = carsSpeed[item.Key] * Time.deltaTime;
                    if (UpdateMove(item, step) == 0)
                    {
                        break;
                    }
                }
            }
        }

    }

    private int UpdateMove(KeyValuePair<Transform, List<Vector2>> item, float step)
    {
        int options = -1;
        var currentCar = item.Key;
        var positionsList = item.Value;
        float oldRotationZ = carsPosition[currentCar];
        
        //only move when have new positions
        if(positionsList.Count == 0)
        {
            return 1;
        }

        Debug.Log("old Z: " + oldRotationZ);
        //convert axisZ to degree so can defind where is the head of the car in CheckDirection
        options = CheckDirection(oldRotationZ, currentCar, positionsList[0]);

        //options 0: forward, 1: turn to upper left, 2: turn to upper right, 3: turn to lower left, 4: turn to lower right
        // 5: moving out to left para slot, 6: moving out to right para slot
        switch (options)
        {
            case 0:
                {
                    currentCar.position = Vector2.MoveTowards(currentCar.position, positionsList[0], step);
                    break;
                }
            case 1:
                {
                    var targetLeft = Quaternion.Euler(new Vector3(0, 0, oldRotationZ + 90));
                    float smooth = 10.0f;
                    currentCar.rotation = Quaternion.Slerp(currentCar.rotation, targetLeft, Time.deltaTime * smooth);

                    if(positionsList[0].y > currentCar.position.y +10)
                    {
                        currentCar.position = Vector2.MoveTowards(currentCar.position, currentCar.position + new Vector3(0, 3f, 0), step);
                    }
                    currentCar.position = Vector2.MoveTowards(currentCar.position, positionsList[0], step);
                    break;
                }
            case 2:
                {
                    var targetRight = Quaternion.Euler(new Vector3(0, 0, oldRotationZ -90));
                    float smooth = 10.0f;
                    currentCar.rotation = Quaternion.Slerp(currentCar.rotation, targetRight, Time.deltaTime * smooth);
                    if (positionsList[0].y > currentCar.position.y + 10)
                    {
                        currentCar.position = Vector2.MoveTowards(currentCar.position, currentCar.position + new Vector3(0, 3f, 0), step);
                    }
                    currentCar.position = Vector2.MoveTowards(currentCar.position, positionsList[0], step);
                    break;
                }
            case 3:
                {
                    var targetLeft = Quaternion.Euler(new Vector3(0, 0, oldRotationZ + 90));
                    float smooth = 35 / Vector2.Distance(currentCar.position, positionsList[0]);
                    currentCar.rotation = Quaternion.Slerp(currentCar.rotation, targetLeft, Time.deltaTime * smooth);

                    if (positionsList[0].y >= currentCar.position.y + 5)
                    {
                        currentCar.position = Vector2.MoveTowards(currentCar.position, currentCar.position + new Vector3(0, -9f, 0), step);
                    }
                    currentCar.position = Vector2.MoveTowards(currentCar.position, positionsList[0], step);
                    break;
                }
            case 4:
                {
                    var targetRight = Quaternion.Euler(new Vector3(0, 0, oldRotationZ - 90));
                    float smooth = 35 / Vector2.Distance(currentCar.position, positionsList[0]);
                    currentCar.rotation = Quaternion.Slerp(currentCar.rotation, targetRight, Time.deltaTime * smooth);

                    if (positionsList[0].y >= currentCar.position.y + 5)
                    {
                        currentCar.position = Vector2.MoveTowards(currentCar.position, currentCar.position + new Vector3(0, -9f, 0), step);
                    }
                    currentCar.position = Vector2.MoveTowards(currentCar.position, positionsList[0], step);
                    break;
                }

        }

        //convert axisZ to degree so can defind where is the head of the car
        int degree = RoundOff(currentCar.eulerAngles.z);
        Debug.Log("new Z: " + degree);
        //delete moved positions and when done, the car moving status set to false
        double distance = Math.Round(Vector2.Distance(currentCar.position, positionsList[0]), 0);
        if (distance == 0 && (degree == 0 || degree == 90 || degree == 180|| degree == 270))
        {
            Debug.Log("Delete");
            positionsList.RemoveAt(0);
            carsPosition[currentCar] = degree;
            if (positionsList.Count == 0)
            {
                movingCars[currentCar] = false;
                Debug.Log("Delete runing car ");
            }
        }
        //if hit gate, car will disapper
        if (Vector2.Distance(currentCar.position, gate1.transform.position) < 5 || Vector2.Distance(currentCar.position, gate2.transform.position) < 5 ||
            Vector2.Distance(currentCar.position, gate3.transform.position) < 5 || Vector2.Distance(currentCar.position, gate4.transform.position) < 5)
        {
            carsSpeed[currentCar] /= 2;
            Vector2 position = currentCar.position;
            currentCar.position = Vector2.MoveTowards(position, position + new Vector2(50,50), step);
            new WaitForSeconds(1);
            currentCar.gameObject.SetActive(false);
            Debug.Log("Gate");
            cars.Remove(currentCar);
            carsPosition.Remove(currentCar);
            movingCars.Remove(currentCar);
            carsSpeed.Remove(currentCar);
            return 0;
        }
        return 1;
    }

    //find the right direction for car each time it moves
    private int CheckDirection(float degree, Transform currentCar , Vector2 newPosition)
    {
        float carX = (float)Math.Round(currentCar.position.x, 0);
        float carY = (float)Math.Round(currentCar.position.y, 0);
        float newX = (float)Math.Round(newPosition.x, 0);
        float newY = (float)Math.Round(newPosition.y, 0);

        //car's head to north
        if (degree ==0 || degree == 360)
        {
            if (carX > newX + 5)
            {
                Debug.Log("y+1");
                return 1;
            }
            else if(carX < newX - 5)
            {
                Debug.Log("y+2");
                return 2;
            }
        }
        //car's head to east
        else if (degree == 270)
        {
            if (carY > newY + 5)
            {
                Debug.Log("x+2 " + carY+": "+ newY);
                return 2;
            }
            else if(carY < newY - 5)
            {
                Debug.Log("x+1" + carY + ": " + newY);
                return 1;
            }
        }
        //car's head to south
        else if (degree == 180)
        {
            if (carX > newX + 5)
            {
                Debug.Log("y-2");
                return 2;
            }
            else if (carX < newX - 5)
            {
                Debug.Log("y-1");
                return 1;
            }
        }
        //car's head to west
        else if (degree == 90)
        {
            if (carY > newY + 5)
            {
                Debug.Log("x-1");
                return 1;
            }
            else if (carY < newY - 5)
            {
                Debug.Log("x-2");
                return 2;
            }
        }
        //moving on the same line
        else if (carX == newX || carY == newY)
        {
            Debug.Log("xy-0");
            return 0;
        }
        Debug.Log("xy-0");
        return 0;
    }

    //Round off the number to 10*
    private int RoundOff(float i)
    {
        return ((int)Math.Round(i / 10.0)) * 10;
    }
    //click on out button
    public void MovingOut()
    {
        cars.Add(car, new List<Vector2>());

        isMovingOut = true;
        isMoving = true;
        ControlUI.SetActive(false);
        OKButton.SetActive(true);
    }
    //add middle point for car to move
    public void addTemp()
    {
        isMoving = true;
        ControlUI.SetActive(false);
        OKButton.SetActive(true);
    }
    // finish temping points
    public void DoneTemp()
    {
        isMoving = false; 
        ControlUI.SetActive(true);
        OKButton.SetActive(false);
        CheckCarIsOut();
    }

    private void OpenControl()
    {
        isControlOpen = true;
        ControlUI.SetActive(true);
        CheckCarIsOut();
    }
    //check if a car is alread out or not
    private void CheckCarIsOut()
    {
        Button[] btns = ControlUI.GetComponentsInChildren<Button>();
        if (cars.ContainsKey(car))
        {
            btns[0].enabled = false;
        }
        else
        {
            btns[0].enabled = true;
        }
    }
    //when click on delete button
    public void ClickDelete()
    {
        ControlUI.SetActive(false);
        isControlOpen = false;
        cars[car] = new List<Vector2>();
        if (!carsPosition.ContainsKey(car))
        {
            cars.Remove(car);
        }
    }
    //when click on accept button
    public void ClickAccept()
    {
        ControlUI.SetActive(false);
        isControlOpen = false;
    }
    //when click back button
    public void ClickBack()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
    //show free slot that car can move to
    private void ShowSlot()
    {

    }
    
    //update drop down as player choose new car
    public void UpdateDropDown()
    {
        Dropdown dropdown = StartUI.GetComponentsInChildren<Dropdown>()[0];
        List<Dropdown.OptionData> messages = dropdown.options;
        dropdown.RefreshShownValue();

        foreach (KeyValuePair<Transform, List<Vector2>> item in cars)
        {
            bool temp = false;
            if (item.Value.Count > 0)
            {
                var newData = new Dropdown.OptionData();
                newData.text = item.Key.gameObject.name;
                foreach(Dropdown.OptionData message in messages)
                {
                    if (message.text.Equals(newData.text))
                    {
                        temp = true;
                        continue;
                    }
                }
                if (!temp)
                {
                    dropdown.options.Add(newData);
                    Debug.Log("add to dropdown");
                }
            }
        }

    }

    //move when click on Go button
    public void StartToMove()
    {
        InputField input = StartUI.GetComponentsInChildren<InputField>()[0];
        Dropdown dropdown = StartUI.GetComponentsInChildren<Dropdown>()[0];
        var selected = dropdown.captionText;
        KeyValuePair<Transform, List<Vector2>> item;
        foreach (KeyValuePair<Transform, List<Vector2>> c in cars)
        {
            if (selected.text == c.Key.gameObject.name)
            {
                item = c;
                break;
            }
        }
        float speedText = float.Parse(input.text);
        if (carsSpeed.ContainsKey(item.Key))
        {
            carsSpeed[item.Key] = speedText;
        }
        else
        {
            carsSpeed.Add(item.Key, speedText);
        }

        //check if moving already had this car 
        if (!carsPosition.ContainsKey(item.Key))
        {
            carsPosition.Add(item.Key, item.Key.eulerAngles.z);
        }
        if (!movingCars.ContainsKey(item.Key))
        {
            movingCars.Add(item.Key, true);
        }
        else if (cars[item.Key].Count > 0)
        {
            movingCars[item.Key] = true;
        }
    }
    //when click startUI
    public void OpenStartUI()
    {
        StartUI.SetActive(!isOpenStartUI);
        isOpenStartUI = !isOpenStartUI;
    }
}
