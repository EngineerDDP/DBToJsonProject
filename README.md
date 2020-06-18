# 离线数据库数据导出软件

本客户端需要可用的数据库服务器链接，并不能自行存储或组织数据。  
客户端用作从数据库里按照指定格式以一个或多个查询条件查询所有关联的数据，并导出为Json文件。
Json树形结构按照数据库实体关联关系建立，弱实体作为强实体的子节点存在。

## 界面
客户端界面使用WPF框架XAML编写，遵循MVVM设计原则。
### 程序窗体
程序窗体在 /Views/WorkSpace/WorkWindow.xaml 中定义，程序窗体指定了菜单栏、登录信息、状态栏和进度条四部分，程序窗体中央为工作区。
基本布局如下所示:

![主界面布局](https://github.com/EngineerDDP/DBToJsonProject/raw/master/.readme/mainWindow.png)

工作区声明如下：
```
<Frame Name="Frame_MainWorkSpace" Grid.Row="1" NavigationUIVisibility="Hidden"/>
```
工作区使用Frame建立，因为导航路径是有限的，因此未使用栈进行页面导航。  
页面导航关系如下图：

![页面导航关系](https://github.com/EngineerDDP/DBToJsonProject/raw/master/.readme/pageNav.png)

### 登录窗口
当自动登录未设置时，登录窗口是程序运行开始第一个显示的窗口。登录界面仿照烂大街登录界面设计。如下图：

![登录窗口](https://github.com/EngineerDDP/DBToJsonProject/raw/master/.readme/loginWindow.png)

### 欢迎页面

欢迎页面为工作区默认页面。  
欢迎页面的帮助手册为*rtf*格式，存储在程序根目录，命名为*help.rtf*。

![欢迎页面](https://github.com/EngineerDDP/DBToJsonProject/raw/master/.readme/welcomePage.png)

### 导入界面

数据库导入相关逻辑代码未在本程序中定义，相关代码使用java编写，并在本程序中通过子进程方式调用。

![导入界面](https://github.com/EngineerDDP/DBToJsonProject/raw/master/.readme/importPage.png)

### 导出界面

数据库导出中，视频和图片导入导出由java子进程决定，该选项仅仅作为指令发送给java子进程，本程序内不做具体处理。  
数据库导出选项栏由程序配置信息自动生成，每个选项均配置一个复选框，有选中/取消两个状态。

## 配置功能

### 开发人员选项
当*菜单栏>开发人员选项>简单模式*未被选中的时候，程序会显示*开发人员选项*菜单。  
当*菜单栏>开发人员选项>简单模式*被选中后，开发人员选项会隐藏，要再次开启该选项，请手动编辑程序设置文件：
```
DEBUG: $WORKINGDIRECTORY$/settings/dbsetting/setting.xml
RELEASE: $SYSAPPDATA$/DataSynchronization_MW/Profiles/dbsetting/setting.xml

<!-- 将配置项 Setting>SimpleMode 改为 False -->
<Settings ActiveUser="" ExportFolder="ExportResult/" SimpleMode="False">
```

要配置数据库信息，转到*>开发人员选项>数据库表选项*，进入数据库配置页面。  
本选项卡中的所有配置信息以xml的形式保存在
```
DEBUG: $WORKINGDIRECTORY$/settings/dbsetting/setting.xml
RELEASE: $SYSAPPDATA$/DataSynchronization_MW/Profiles/dbsetting/setting.xml
```

#### 常规
**常规**页面包含了数据库连接字符串的配置，可以在*数据库连接字符串*栏直接显式指定用户名与密码进行连接。  
**用户验证设置**中配置离线用户验证相关信息。
#### 导入设置
导入配置相关文件由java子程序处理，本界面配置信息仅作备份。
#### 导出设置
**数据库选项**中数据库连接字符串用于作为局部变量覆盖全局连接字符串，当在本页面配置了数据库连接字符串后，全局数据库链接将被替换。
**Json**文件结构中列举了当前配置环境下最终生成的Json文件结构，以及Json属性的名称。
将数据库实体关系设定好后，点保存检查数据库配置是否正确，如果存在关系指定错误，程序将会弹窗提示。如果未出现检查错误，程序将会提示保存成功。  
正确配置数据库实体关系后，应该如下图所示：

![保存成功](https://github.com/EngineerDDP/DBToJsonProject/raw/master/.readme/dbSaved.png)

当程序在做实体关系检查时发生错误时，程序会报告出现错误的实体位置。

![检查未通过](https://github.com/EngineerDDP/DBToJsonProject/raw/master/.readme/errorBox.png)



