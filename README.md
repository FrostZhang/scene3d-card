# scene3d-card
homeassistant 3D方案

[![hacs_badge](https://img.shields.io/badge/HACS-Default-orange.svg?style=for-the-badge)](https://github.com/hacs/integration)
[![License][license-shield]](LICENSE.md)
![Project Maintenance][maintenance-shield]

https://user-images.githubusercontent.com/27171600/170735287-e8afea8d-e399-4a9a-a795-1876636c99ee.mp4
### 硬件要求
+ 安装无任何要求，树莓、群晖、手机、linux随意。
+ PC浏览：GT750   RX550   HD6870  +4G内存 及以上
+ 移动端浏览：麒麟980 A10X 晓龙778  及以上

### 软件要求
+ 推荐 Chrome、Edge
+ ~~仅支持系统Lovelace仪表盘~~（注：v1.0.5以后支持用户自定义仪表盘）
### 安装
+ [release](https://github.com/FrostZhang/hass_unity_home/releases)包，放在容器中。
+ StreamingAssets.zip 资源包解压放在custom_components\3dscene\local\StreamingAssets同名目录中。
+ 确保custom_components\3dscene\local\Customdata 可读写！！！
+ 重启容器
+ configuration.yaml文件添加 3dscene: （注意后面有空格）
+ 重启容器
+ 首页仪表盘新建一个分类，选择 面板-单张卡片（注意：请不要多张卡片混排，三维太吃性能，易发生浏览器崩溃）
+ 新建卡片 type: custom:scene3d-card

### 打赏
<img src="http://cdn.asherlink.top/wxskm.jpg" width="128"  alt="微信"/> ![wx](/Other/wxskm.jpg) <a href="https://www.buymeacoffee.com/3762375193" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/white_img.png" alt="Buy Me A Coffee" style="height: auto !important;width: auto !important;" ></a>

## 使用
+ 看说明书
+ 中国用户可以登录bilibili关注：锯木工_Asher 
+ 
## 问答
+ 第一次加载有点慢？
- 三维资源很大，本体10M，资源大约16M（持续更新），网页打开受网速限制，浏览器会缓存所有资源。
-
+ 手机上效果怎么样？
- 移动端: SMAA（-）Anti（2x）HDR（-）Bloom辉光（-）LUT（16）阴影（Hight）Light（8）
- PC: SMAA（Hight）Anti（2x）HDR（+）Bloom辉光（+）LUT（32）阴影（-）Light（8）
- 简单的说PC端画质全开，移动端画质开一半。
-
+ 卡顿，报错xxx out memory，长时间黑屏。
- 更换设备。
-
+ 为什么整个世界很黑？
- 世界受homeassistant sun.sun影响，根据当前太阳的角度计算光照强度。
- 简单的说，世界有白天和夜晚跟现实一致。
-
+ 怎么下雨雪？
- 配置weather entity，世界跟随现实天气变幻。
- 中国用户建议接入和风天气 hf_weather 插件。
-
+ 编辑页面 save config，homeassistant报错？
- 可能 custom_components\3dscene\local\Customdata 没有读写权限，没法在 save config 时保存你的配置。
-
+ 我编辑后，刷新页面没变化？
- 可能 custom_components\3dscene\local\Customdata 没有读写权限，没法在 save config 时保存你的配置。
- 可能你没有点击 save config 按钮
-
+ 编辑左侧画风诡异？
- 预览图片无灯光渲染就是这样啦，放在场景里就好看了。以后有空会慢慢优化的。
-

### 我想开发卡片
+ 编辑custom_components - 3dscene - local - 3dscene-card.js
+ 编辑3dscene下的文件

## 广告（订制开发）
+ 定制homeassistant个性化三维场景和个性化功能。
+ 三维交互式平台做为独立模块可嵌入其他智能家居app、网站、电脑软件。
+ 
# 遇到bug不要慌，欢迎留言，B站私信。

[license-shield]: https://img.shields.io/github/license/custom-cards/restriction-card.svg?style=for-the-badge
[maintenance-shield]: https://img.shields.io/badge/maintainer-Ian%20Richardson%20%40iantrich-blue.svg?style=for-the-badge
