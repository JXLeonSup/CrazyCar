﻿using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;
using TFramework;

public class UISceneLoadingCtrl : MonoBehaviour, IController {
    public Slider progressSlider;
    public Text progressText;

    private float timer;
    public float minLoadingTime = 3f;

    private void Start() {
        this.GetModel<IGameControllerModel>().SceneLoaded.Value = false;
        progressSlider.value = 0;
        progressText.text = "0%";
        CoroutineController.manager.StartCoroutine(LoadScene());
        DontDestroyOnLoad(gameObject);
    }

    private void Update() {
        timer += Time.deltaTime;
    }

    private IEnumerator LoadScene() {
        this.GetModel<IGameControllerModel>().SceneLoaded.Value = false;
        progressSlider.value = 0;
        progressText.text = (int)(progressSlider.value * 100) + "%";
        progressSlider.value = 0.1f;
        progressText.text = (int)(progressSlider.value * 100) + "%";
        var async = SceneManager.LoadSceneAsync((int)Util.LoadingTargetSceneID);

        while (timer < minLoadingTime) {
            var maxProgress = (timer / minLoadingTime);
            progressSlider.value = maxProgress;
            progressText.text = (int)(maxProgress * 100) + "%";
            yield return null;
        }

        while (!async.isDone) {
            progressSlider.value = Mathf.Min(async.progress, async.progress);
            progressText.text = (int)(progressSlider.value * 100) + "%";
            yield return null;
        }
        // 2019加载完场景并不能直接显示
        Destroy(gameObject);
        yield return new WaitForSeconds(1.8f);
        this.GetModel<IGameControllerModel>().SceneLoaded.Value = true;
        this.SendCommand(new SelectGameUICommand());
    }

    public IArchitecture GetArchitecture() {
        return CrazyCar.Interface;
    }
}