# hass_unity_home
<iframe src="//player.bilibili.com/player.html?aid=299475993&bvid=BV1pF41157wK&cid=731431313&page=1" scrolling="no" border="0" frameborder="no" framespacing="0" allowfullscreen="true"> </iframe>
### 硬件要求
+ 安装无任何要求，树莓、群晖、手机、linux随意。
+ PC浏览：GT750   RX550   HD6870  +4G内存 及以上
+ 移动端浏览：麒麟980 A10X 晓龙778  及以上

### 软件要求
+ Chrome、Edge，其他浏览器未通过测试。
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
![wx](http://cdn.asherlink.top/wxskm.jpg) ![wx](/Other/wxskm.jpg)

## 使用
+ 看说明书
+ 或者b站 关注 锯木工_Asher 
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
+ 有点卡，浏览器报 xxx out memory ，长时间黑屏。
- 更换演示设备。
-
+ 为什么整个世界很黑？
- 世界受homeassistant sun.sun影响，根据当前太阳的角度计算光照强度。
- 简单的说，世界有白天和夜晚跟现实一致。
-
+ 怎么下雨雪？
- 配置weather entity，世界跟随现实天气变幻。
- 建议接入和风天气 hf_weather 插件。
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
### 我想开发三维
+ 为便于小白使用插件，开发文件迁移。
+ 开发条件：具备 unity c# js 3d运算 开发经验。

### 我想开发卡片
+ 编辑custom_components - 3dscene - local - 3dscene-card.js
+ 编辑3dscene下的文件

## 广告（订制开发）
+ 定制homeassistant个性化三维场景和个性化功能。
+ 三维交互式平台做为独立模块可嵌入其他智能家居app、网站、电脑软件。
+ 有意私信。
# 遇到bug不要慌，欢迎留言，B站私信。
