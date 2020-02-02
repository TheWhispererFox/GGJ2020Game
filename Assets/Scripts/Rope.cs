using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float segmentLength;

    [Header("References")]
    [SerializeField] private Transform head;
}
