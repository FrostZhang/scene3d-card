// 延时加载，解决每次界面显示不了的问题
; (() => {
  const timer = setInterval(() => {
    if (Polymer.Element) {
      clearInterval(timer);
      // 开始生成DOM元素
      class UnityCard extends Polymer.Element {

        static get template() {
          return Polymer.html`
          <link rel="stylesheet" href="/3dscene_local/TemplateData/style.css">
          <ha-card>
          <div id="unity-container" class="unity-desktop">
              <canvas id="unity-canvas" width=960 height=600></canvas>
              <div id="unity-loading-bar">
                  <div id="unity-logo"></div>
                  <div id="unity-progress-bar-empty">
                      <div id="unity-progress-bar-full"></div>
                  </div>
              </div>
              <div id="unity-warning"> </div>
              <div id="unity-footer">
                  <!-- <div id="unity-webgl-logo"></div> -->
                  <!-- <div id="unity-fullscreen-button"></div> -->
                  <!-- <div id="unity-build-title">HassHome</div> -->
              </div>
          </div>
          </ha-card>
          [[test()]]
    `;
        }

        static get properties() {
          return {
            config: Object,
            asherlink3dscence: Object,
            sunObj: Object,
            tempObj: Object,
            mode: String,
            weatherObj: {
              type: Object,
              observer: 'dataChanged',
            },
          };
        }

        constructor() {
          super();

        }


        // 自定义默认配置
        static getStubConfig() {
          //return { entity: "weather.tian_qi" }
        }

        setConfig(config) {
          this.config = config;
          //放弃lovelace 配置表，原因：手动编辑lovelace反人类
          //if (this.asherlink3dscence == undefined) return;
          //this.asherlink3dscence.SendMessage("Shijie", "HassConfig", JSON.stringify(this.config));
        }

        configChanged(newConfig) {
          const event = new Event("config-changed", {
            bubbles: true,
            composed: true
          });
          event.detail = { config: newConfig };
          this.dispatchEvent(event);
        }

        set hass(hass) {
          this._hass = hass;
          this.lang = this._hass.selectedLanguage || this._hass.language;
          //回传3d
          window.asherlink3dscencehass = this._hass;
          if (this.asherlink3dscence == undefined) return;
          var st, item, send;
          //遍历全部state，考虑优化
          for (item in this._hass.states) {
            st = this._hass.states[item];
            if (item == "sun.sun") {
              send = item + ' ' + st["attributes"]["elevation"];
            }
            else if (item.startsWith("weather")) {
              //和风天气
              if (st.hasOwnProperty("attributes") && st["attributes"].hasOwnProperty("condition_cn")) {
                send = item + ' ' + st["attributes"]["condition_cn"];
              }
              else {
                send = item + ' ' + st["state"];
              }
            }
            else {
              send = item + ' ' + st["state"];
            }
            if (send != undefined) {
              this.asherlink3dscence.SendMessage("Shijie", "HassMessage", String(send));
            }
          }
        }

        test() {
          setTimeout(() => {
            var container = this.shadowRoot.querySelector("#unity-container");
            var canvas = this.shadowRoot.querySelector("#unity-canvas");
            var loadingBar = this.shadowRoot.querySelector("#unity-loading-bar");
            var progressBarFull = this.shadowRoot.querySelector("#unity-progress-bar-full");
            var warningBanner = this.shadowRoot.querySelector("#unity-warning");
            function unityShowBanner(msg, type) {
              function updateBannerVisibility() {
                warningBanner.style.display = warningBanner.children.length ? 'block' : 'none';
              }
              var div = document.createElement('div');
              div.innerHTML = msg;
              warningBanner.appendChild(div);
              if (type == 'error') div.style = 'background: red; padding: 10px;';
              else {
                if (type == 'warning') div.style = 'background: yellow; padding: 10px;';
                setTimeout(function () {
                  warningBanner.removeChild(div);
                  updateBannerVisibility();
                }, 5000);
              }
              updateBannerVisibility();
            }
            var buildUrl = "/3dscene_local/Build";
            var loaderUrl = buildUrl + "/prize.loader.js";
            var config = {
              dataUrl: buildUrl + "/prize.data.unityweb",
              frameworkUrl: buildUrl + "/prize.framework.js.unityweb",
              codeUrl: buildUrl + "/prize.wasm.unityweb",
              streamingAssetsUrl: "3dscene_local/StreamingAssets",
              companyName: "AsherLink",
              productName: "scene3d-card",
              productVersion: "1.0.3",
              showBanner: unityShowBanner,
            };
            if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
              container.className = "unity-mobile";
              // Avoid draining fillrate performance on mobile devices,
              // and default/override low DPI mode on mobile browsers.
              config.devicePixelRatio = 2;
              //unityShowBanner('');
            } else {
              container.className = "unity-mobile";
              config.devicePixelRatio = 1;
              //canvas.style.width = "960px";
              //canvas.style.height = "600px";
            }
            loadingBar.style.display = "block";
            var customss = document.createElement("script");
            customss.text = `
            var asherlink3dscencehass,asherlink3dscencecard;
            function AsherLink3DStart(){
              if (window.asherlink3dscencecard != undefined)
              window.asherlink3dscencecard.On3dstart();
            }
            function AsherLink3DClickMessage(str){
              var obj = JSON.parse(str);
              asherlink3dscencehass.callService(obj["head"],obj["cmd"],{"entity_id": obj["entity_id"]});
            }
            function AsherLinkfire(type, detail, options) {
              const node = asherlink3dscencecard;
              options = options || {};
              detail = (detail === null || detail === undefined) ? {} : detail;
              const e = new Event(type, {
                bubbles: options.bubbles === undefined ? true : options.bubbles,
                cancelable: Boolean(options.cancelable),
                composed: options.composed === undefined ? true : options.composed
              });
              e.detail = detail;
              node.dispatchEvent(e);
              return e;
            }
            function AsherLink3DLongClickMessage(str){
              var obj = JSON.parse(str);
              AsherLinkfire('hass-more-info', { entityId: obj["entity_id"] });
            }
            function AsherLink3DWebLog(str){
             console.log(str)
            }
            function AsherLink3DConfig(str){
              var obj = JSON.parse(str);
              asherlink3dscencehass.callService('3dscene','config',{'data': obj});
            }
            `
            document.body.appendChild(customss)
            var script = document.createElement("script");
            script.src = loaderUrl;
            script.onload = () => {
              createUnityInstance(canvas, config, (progress) => {
                progressBarFull.style.width = 100 * progress + "%";
              }).then((unityInstance) => {
                loadingBar.style.display = "none";
                this.asherlink3dscence = unityInstance;
                window.asherlink3dscencecard = this;
                
            }).catch((message) => {
                alert(message);
              });
            };
            document.body.appendChild(script)
          }, 1000)

        }

        On3dstart() {
          //放弃从lovelace推送配置，原因：手动编辑lovelace反人类
          //console.log(JSON.stringify(this.config))
          //this.asherlink3dscence.SendMessage("Shijie", "HassConfig", JSON.stringify(this.config));
        }

        getCardSize() {
          return 4;
        }

        _fire(type, detail, options) {
          const node = this.shadowRoot;
          options = options || {};
          detail = (detail === null || detail === undefined) ? {} : detail;
          const e = new Event(type, {
            bubbles: options.bubbles === undefined ? true : options.bubbles,
            cancelable: Boolean(options.cancelable),
            composed: options.composed === undefined ? true : options.composed
          });
          e.detail = detail;
          node.dispatchEvent(e);
          return e;
        }
      }

      customElements.define('scene3d-card', UnityCard);

      // 添加预览
      window.customCards = window.customCards || [];
      window.customCards.push({
        type: "scene3d-card",
        name: "homeassistant 3D",
        preview: false,
        description: "Build your home -- AsherLink"
      });

    }
  }, 1000)
})();
