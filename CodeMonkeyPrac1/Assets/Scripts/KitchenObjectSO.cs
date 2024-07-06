using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class KitchenObjectSO : ScriptableObject {
    //If you are going to write to a SO, make it SerializeField
    //Instead of public, and use a get function
    public Transform prefab;
    public Sprite sprite;
    public string objectName;

}