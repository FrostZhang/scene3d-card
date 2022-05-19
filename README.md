# hass_unity_home

### 安装
+ 本项目的custom_components - 3dscene放入容器（可下载release包或整个项目）
+ 重启容器
+ 首页仪表盘新建一个分类，选择 面板（单张卡片）
+ 新建卡片 type: custom:scene3d-card

### 开发三维
+ 下载项目
+ 使用unity3d打开
+ 自定义程序
+ 编辑webgl导出包
+ 替换Build文件夹和StreamingAssets文件夹

### 开发卡片
+ 编辑custom_components - 3dscene - local - 3dscene-card.js
+ 编辑3dscene下的文件

## 问答
+ 我的界面是黑屏
- 三维资源很大，本体10M，资源大约16M（持续更新），浏览器第一次加载需要时间。如果10分钟仍然黑屏请按F12将浏览器调试界面发给我。

+ 手机打开非常模糊
- 本项目暂不支持在Android iPhone 显示，请考虑floor3D等方案。

