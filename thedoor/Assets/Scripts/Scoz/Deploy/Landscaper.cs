using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scoz.Func
{
    public class Landscaper : MonoBehaviour 
    {
        public bool AutoSpawn = true;
        public Vector3 SpawnPlatePos = Vector3.zero;
        public bool CenterSpawn = false;
        public bool XDirReverse = false;
        public bool YDirReverse = false;
        public bool ZDirReverse = false;
        public int XPlateNum = 10;
        public int YPlateNum = 10;
        public int ZPlateNum = 10;
        public Transform ParentTrans;
        public Vector3 PlateSize = new Vector3(0, 0, 0);
        public Vector3 Spacing = new Vector3(0, 0, 0);
        public GameObject PlatePrefab;
        public int PlateCount
        {
            get
            {
                return XPlateNum * YPlateNum * ZPlateNum;
            }
        }



        // Start is called before the first frame update
        protected virtual void Start()
        {
            if (AutoSpawn)
                BuildPlates();
        }

        public virtual void BuildPlates()
        {
            int dirX = (XDirReverse) ? -1 : 1;
            int dirY = (YDirReverse) ? -1 : 1;
            int dirZ = (ZDirReverse) ? -1 : 1;

            if (CenterSpawn)
            {
                SpawnPlatePos = new Vector3(dirX * Mathf.CeilToInt(XPlateNum / 2) * (Spacing.x + PlateSize.x), dirY * Mathf.CeilToInt(YPlateNum / 2) * (Spacing.y + PlateSize.y), dirZ * Mathf.CeilToInt(ZPlateNum / 2) * (Spacing.z + PlateSize.z));
            }
            for (int a = 0; a < XPlateNum; a++)
            {
                for (int b = 0; b < YPlateNum; b++)
                {
                    for (int c = 0; c < ZPlateNum; c++)
                    {
                        GameObject go = Instantiate(PlatePrefab);
                        if (ParentTrans)
                            go.transform.SetParent(ParentTrans);
                        go.transform.localRotation = PlatePrefab.transform.localRotation;
                        go.transform.localPosition = new Vector3(SpawnPlatePos.x + (PlateSize.x * a + a * Spacing.x), SpawnPlatePos.y + (PlateSize.y * b - b * Spacing.y), SpawnPlatePos.z + (PlateSize.z * c + c * Spacing.z));
                    }
                }
            }
        }
    }
}
