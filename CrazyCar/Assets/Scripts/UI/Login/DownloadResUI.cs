﻿using LitJson;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using System;
using TFramework;

public class DownloadResUI : MonoBehaviour, IController {
    public Text showText;
    public Slider progressSlider;
    public Button standAloneBtn;

    private IResourceSystem resourceSystem;

    private void Start() {
        resourceSystem = this.GetSystem<IResourceSystem>();

        standAloneBtn.onClick.AddListener(() => {
            this.SendCommand<EnableStandAloneCommand>();
        });

#if !UNITY_EDITOR
    CheckResource(() => {
        this.SendCommand(new DownloadResFinishCommand());
    });
#else
        StringBuilder sb = new StringBuilder();
        JsonWriter w = new JsonWriter(sb);
        w.WriteObjectStart();
        w.WritePropertyName("platform");
        w.Write(Util.GetPlatform());
        w.WritePropertyName("version");
        w.Write(Application.version);
        w.WriteObjectEnd();
        Debug.Log("++++++ " + sb.ToString());
        Debug.Log("++++++ " + this.GetSystem<INetworkSystem>().HttpBaseUrl);
        byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
        StartCoroutine(this.GetSystem<INetworkSystem>().POSTHTTP(url: this.GetSystem<INetworkSystem>().HttpBaseUrl + RequestUrl.forcedUpdatingUrl,
            data: bytes, succData: (data) => {
                if ((bool)data["is_forced_updating"]) {
                    this.GetModel<IGameControllerModel>().InfoConfirmAlert.ShowWithText(content: this.GetSystem<II18NSystem>().GetText("Version is too low"),
                        success: () => {
                            Application.OpenURL((string)data["url"]);
                            Application.Quit();
                        },
                        confirmText: this.GetSystem<II18NSystem>().GetText("Download"));
                } else {
                    CheckResource(() => {
                        this.SendCommand(new DownloadResFinishCommand());
                    });
                }
            }));
#endif
    }

    public void CheckResource(Action success) {
        StartCoroutine(Check(success));
    }

    private IEnumerator Check(Action success) {
        yield return new WaitUntil(() => {
            return resourceSystem != null;
        });
        showText.text = "";
        progressSlider.value = 0;
        resourceSystem.CheckNewResource();
        yield return new WaitUntil(() => {
            return resourceSystem.curResourceType != ResourceType.None;
        });
        if (resourceSystem.curResourceType != ResourceType.None) {
            resourceSystem.DownloadAssets(() => {
                success?.Invoke();
            }, UpdateProgress, () => {
                this.GetModel<IGameControllerModel>().WarningAlert.ShowWithText(text: this.GetSystem<II18NSystem>().GetText("Download assets failed"), callback: () => {
                    Application.Quit();
                });
            });
        } else {
            success?.Invoke();
        }
    }

    private float lastProgress = 0;
    private float lastTime = -1;
    private void UpdateProgress(float value, float totalBytes, bool isDownloading) {
        if (value < 0.01f) {
            return;
        }

        if (isDownloading) {
            // download
            var t = Time.realtimeSinceStartup;
            float speed = 0f;
            try {
                speed = ((value - lastProgress) * totalBytes) / (t - lastTime);
                speed /= 1024;
            } catch {
            }

            if (lastTime < 0) {
                speed = 0f;
                lastTime = t;
            }

            progressSlider.value = value;
            if (speed > 1024) {
                showText.text = (speed / 1024).ToString("0.0") + "M/s";
            } else {
                showText.text = speed.ToString("0.0") + "K/s";
            }
        } else {
            showText.text = this.GetSystem<II18NSystem>().GetText("Resource loading");
        }
    }

    public IArchitecture GetArchitecture() {
        return CrazyCar.Interface;
    }
}
