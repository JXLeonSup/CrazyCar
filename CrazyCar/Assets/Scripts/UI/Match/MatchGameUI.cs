﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using TFramework;

public class MatchGameUI : MonoBehaviour, IController {
    public Text offStartTimeText;
    public CountDownAnim countDownAnim;
    public Text limitTimeText;

    private Coroutine limitTimeCor;

    private void OnEnable() {
        if (!this.GetModel<IGameControllerModel>().SceneLoaded.Value) {
            return;
        }

        int offTime = (int)(this.GetModel<IMatchModel>().StartTime * 1000 - Util.GetTime()) / 1000;
        if (offTime > 3){
            StartCoroutine(CountdownCor(offTime,
                    () => {
                        countDownAnim.gameObject.SetActiveFast(false);
                        StartGame();
                    }, offStartTimeText));
        } else if(offTime < 3 && offTime > 0) {
            countDownAnim.PlayAnim(offTime, () => {
                StartGame();
            });
        } else if (offTime > -this.GetModel<IMatchModel>().SelectInfo.Value.limitTime){
            countDownAnim.gameObject.SetActiveFast(false);
            StartGame();
        } else {
            countDownAnim.gameObject.SetActiveFast(false);
            StartGame();
            Debug.LogError("比赛结束");
        }
        
    }

    private void StartGame() {
        this.GetSystem<IPlayerManagerSystem>().SelfPlayer.vInput = 1;
        Debug.Log("++++++ StartTime = " + this.GetModel<IMatchModel>().StartTime);
        limitTimeCor = StartCoroutine(CountdownCor(this.GetModel<IMatchModel>().SelectInfo.Value.limitTime,
            () => {
                this.GetModel<IMatchModel>().IsArriveLimitTime.Value = true;
                Debug.Log("++++++ arrive limit time ");
            }, limitTimeText));
    }

    private void Start() {
        limitTimeText.text = this.GetModel<IMatchModel>().SelectInfo.Value.limitTime.ToString();

        this.RegisterEvent<CompleteMatchEvent>(OnCompleteMatch);
    }

    private void OnCompleteMatch(CompleteMatchEvent e) {
        StopCoroutine(limitTimeCor);
        this.SendCommand(new ShowResultUICommand());
    }

    private IEnumerator CountdownCor(int time, Action succ = null, Text targetText = null, string str = null) {
        while (true) {
            if (targetText != null) {
                if (str != null) {
                    targetText.text = string.Format(str, time);
                } else {
                    targetText.text = time.ToString();
                }
            }

            yield return new WaitForSecondsRealtime(1.0f);
            time--;
            if (time < 0) {
                succ?.Invoke();
                yield break;
            }
        }
    }

    private void OnDestroy() {
        this.UnRegisterEvent<CompleteMatchEvent>(OnCompleteMatch);
    }

    public IArchitecture GetArchitecture() {
        return CrazyCar.Interface;
    }
}
