using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralSettings : MonoBehaviour
{
    public static GeneralSettings Instance;

    public int Seed;
    public int RoomCount;
    public int EnemyRoomCount;
    public int ItemRoomCount;
    public float sensX;
    public float sensY;


// only one of these settings files are allowed to exist at any given time
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
