using DamageNumbersPro;
using Scoz.Func;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DNPManager : MonoBehaviour {

    [SerializeField] DamageNumber MissDPN;
    [SerializeField] DamageNumber FailDPN;
    [SerializeField] DamageNumber DmgDPN;
    [SerializeField] DamageNumber SanDmgDPN;
    [SerializeField] DamageNumber RestoreDPN;
    [SerializeField] DamageNumber SanRestoreDPN;
    [SerializeField] DamageNumber DizzyDPN;
    [SerializeField] DamageNumber PoisonDPN;
    [SerializeField] DamageNumber InsanityDPN;
    [SerializeField] DamageNumber BleedingDPN;
    [SerializeField] DamageNumber FearDPN;
    [SerializeField] DamageNumber EvadeDPN;
    [SerializeField] DamageNumber CalmDPN;
    [SerializeField] DamageNumber FocusDPN;
    [SerializeField] DamageNumber HorrorDPN;
    [SerializeField] DamageNumber ProtectionDPN;
    [SerializeField] DamageNumber FortitudeDPN;
    [SerializeField] DamageNumber AntidoteDPN;
    [SerializeField] DamageNumber RecoveryDPN;
    [SerializeField] DamageNumber StrongDPN;
    [SerializeField] DamageNumber FaithDPN;
    [SerializeField] DamageNumber FleeDPN;

    public static DNPManager Instance;

    public static bool IsInit { get; private set; }



    public static DNPManager CreateNewInstance() {
        if (Instance != null) {
            WriteLog.LogError("DNPManager之前已經被建立了");
        } else {
            AddressablesLoader.GetPrefab("DNP/DNPManager", (prefab, handle) => {
                GameObject go = Instantiate(prefab);
                go.name = "DNPManager";
                Instance = go.GetComponent<DNPManager>();
                Instance.Init();
            });
        }
        return Instance;
    }


    public enum DPNType {
        Miss, Fail, Dmg, SanDmg, Restore, SanRestore, Dizzy, Poison, Insanity, Bleeding, Fear, Evade, Calm,
        Focus, Horror, Protetion, Fortitude, Antidote, Recovery, Strong, Faith, Flee,
    }
    Dictionary<DPNType, DamageNumber> DPNDic = new Dictionary<DPNType, DamageNumber>();

    public void Init() {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        DPNDic.Add(DPNType.Miss, MissDPN);
        DPNDic.Add(DPNType.Fail, FailDPN);
        DPNDic.Add(DPNType.Dmg, DmgDPN);
        DPNDic.Add(DPNType.SanDmg, SanDmgDPN);
        DPNDic.Add(DPNType.Restore, RestoreDPN);
        DPNDic.Add(DPNType.SanRestore, SanRestoreDPN);
        DPNDic.Add(DPNType.Dizzy, DizzyDPN);
        DPNDic.Add(DPNType.Poison, PoisonDPN);
        DPNDic.Add(DPNType.Insanity, InsanityDPN);
        DPNDic.Add(DPNType.Bleeding, BleedingDPN);
        DPNDic.Add(DPNType.Fear, FearDPN);
        DPNDic.Add(DPNType.Evade, EvadeDPN);
        DPNDic.Add(DPNType.Calm, CalmDPN);
        DPNDic.Add(DPNType.Focus, FocusDPN);
        DPNDic.Add(DPNType.Horror, HorrorDPN);
        DPNDic.Add(DPNType.Protetion, ProtectionDPN);
        DPNDic.Add(DPNType.Fortitude, FortitudeDPN);
        DPNDic.Add(DPNType.Antidote, AntidoteDPN);
        DPNDic.Add(DPNType.Recovery, RecoveryDPN);
        DPNDic.Add(DPNType.Strong, StrongDPN);
        DPNDic.Add(DPNType.Faith, FaithDPN);
        DPNDic.Add(DPNType.Flee, FleeDPN);
    }
    public DamageNumber Spawn(DPNType _type, float _value, RectTransform _parentTrans, Vector2 _anchorPos) {
        Debug.Log("_type=" + _type);
        if (!DPNDic.ContainsKey(_type) || DPNDic[_type] == null) return null;
        Debug.Log("DPNDic[_type]=" + DPNDic[_type]);
        DamageNumber dn = DPNDic[_type].Spawn(Vector3.zero, _value);

        dn.SetAnchoredPosition(_parentTrans, _anchorPos);
        return dn;
    }




}
