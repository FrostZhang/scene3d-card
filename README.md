# hass_unity_home

### 硬件要求
+ 无任何要求
+ 树莓、群晖、手机、linux随意。因为渲染计算不在容器中。

### 安装
+ 下载release包，放在容器中。
+ 资源包放在custom_components\3dscene\local\StreamingAssets中。
+ 确保custom_components\3dscene\local\Customdata 可读写！！！
+ 重启容器
+ 首页仪表盘新建一个分类，选择 面板（单张卡片）
+ 新建卡片 type: custom:scene3d-card

### 打赏
![wx](http://cdn.asherlink.top/wxskm.jpg) ![wx](/Other/wxskm.jpg)

### 我想开发三维
+ 下载项目
+ 使用unity3d打开
+ 自定义程序
+ 编辑webgl导出包，注意导出包的文件夹名称必须为 prize
+ 替换Build文件夹和StreamingAssets文件夹

### 我想开发卡片
+ 编辑custom_components - 3dscene - local - 3dscene-card.js
+ 编辑3dscene下的文件

## 问答
+ 为什么我用浏览器打开显示黑屏？
- 三维资源很大，本体10M，资源大约16M（持续更新），网页打开受网速限制，但是浏览器加载后缓存所有资源。
-
+ 为什么手机打开非常模糊？
- 本项目暂不支持在Android iPhone 高品质显示，请考虑floor3D等方案。
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
+ 编辑测左边画风诡异？
- 预览图片无灯光渲染就是这样啦，放在场景里就好看了。
-
## 编辑器怎么用
+ 自己摸索
+ 或者b站 关注 锯木工_Asher 

# 遇到bug不要慌，欢迎留言，B站私信。
