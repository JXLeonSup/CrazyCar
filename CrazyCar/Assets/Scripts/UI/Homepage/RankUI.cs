﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using TFramework;

public class RankUI : MonoBehaviour, IController {
    public RankDetailItem rankDetailItem;
    public Transform itemParent;
    public Button closeBtn;

    private void OnEnable() {
        StartCoroutine(this.GetSystem<INetworkSystem>().POSTHTTP(url: this.GetSystem<INetworkSystem>().HttpBaseUrl +
            RequestUrl.timeTrialDetailUrl,
           token: this.GetModel<IGameControllerModel>().Token.Value,
           succData: (data) => {
               this.GetModel<ITimeTrialModel>().ParseClassData(data, UpdateUI);
           }));
    }

    private void UpdateUI() {
        Util.DeleteChildren(itemParent);
        foreach (var kvp in this.GetModel<ITimeTrialModel>().TimeTrialDic) {
            RankDetailItem item = Instantiate(rankDetailItem);
            item.transform.SetParent(itemParent, false);
            item.SetContent(kvp.Value);
        }
    }

    private void Start() {
        closeBtn.onClick.AddListener(() => {
            this.SendCommand(new ShowPageCommand(UIPageType.HomepageUI));
        });
    }

    public IArchitecture GetArchitecture() {
        return CrazyCar.Interface;
    }
}
