using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class LoadingScreen : MonoBehaviour
{
    public UnityAction<float> OnPercentageChanged;
    
    public void SetPercentage(float percent)
    {
        OnPercentageChanged?.Invoke(percent);
    }
}
