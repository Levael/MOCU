using System;
using System.Linq;
using System.Text;
using Unity.Profiling;
using UnityEngine;

public class ForTests : MonoBehaviour
{
    private DebugTabHandler _debugTabHandler;

    private void Awake()
    {
        _debugTabHandler = GetComponent<DebugTabHandler>();
    }

    private void Start()
    {
        _debugTabHandler.testBtn1Clicked += (eventObj) => { print(eventObj.currentTarget); };
    }

    private void Update()
    {
        
    }
}
