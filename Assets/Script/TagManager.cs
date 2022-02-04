using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagManager : MonoBehaviour
{
    [SerializeField]
    private GameObject tagPrefab;

    private Transform container = null;

    [System.Serializable]
    class MyTaggedObject
    {
        public string objectTypeName;
        public GameObject tarObject;
        public float indicatorHeight = 0f;
    }

    [SerializeField]
    private List<MyTaggedObject> MyTaggedObjectList;

    private void Awake()
    {
        container = GameObject.Find("TagGroup").transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        BindTagObject();
    }
    
    private void LateUpdate()
    {
        if (container != null)
        {
            if (AppManager.instance.myTestState == AppManager.TestType.Off)
            {
                container.gameObject.SetActive(true);
            }
            else
            {
                container.gameObject.SetActive(false);
            }
        }
    }

    private void BindTagObject()
    {
        if (container != null)
        {
            for (int i = 0; i < MyTaggedObjectList.Count; i++)
            {
                MyTaggedObject obj = MyTaggedObjectList[i];

                GameObject tar = obj.tarObject;

                GameObject go = Instantiate(tagPrefab, tar.transform.position, Quaternion.identity);
                go.name = "Tag_" + i;

                go.transform.Find("Tag Container").localPosition = new Vector3(0, obj.indicatorHeight, 0);

                go.GetComponentInChildren<TextMesh>().text = obj.objectTypeName;

                go.transform.SetParent(container);
            }
        }
    }
}
