using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCollider : MonoBehaviour
{
    public Sprite img;
    // Start is called before the first frame update
    void Awake()
    {
        GetComponent<SpriteRenderer>().sprite = img;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
