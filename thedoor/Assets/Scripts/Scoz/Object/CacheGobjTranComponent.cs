using UnityEngine;
using System.Collections;

public class CacheGobjTranComponent : MonoBehaviour {
	
	private GameObject cacheGameObject;
	public GameObject CacheGameObject{
		get{
			if(this == null)
				return null;
			if(cacheGameObject == null)
				cacheGameObject = gameObject;
			return cacheGameObject;
		}
	}
	
	private Transform cacheTransform;
	public Transform CacheTransform{
		get{
			if(this == null)
				return null;
			if(cacheTransform == null)
				cacheTransform = transform;
			return cacheTransform;
		}
	}

}
