using System;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private bool isRunning = false;
    private float startTime;
    [SerializeField] private float timerDuration = 10f;

    public event Action TimerFinish;

    public void StartTimer()
    {
        startTime = Time.time;
        isRunning = true;
    }
    public void StopTimer()
    {
        isRunning = false;
    }
    private void FixedUpdate()
    {
        if (isRunning && timerDuration <= Time.time - startTime)
        {
            TimerFinish.Invoke();
            StopTimer();
        }
    }
}
