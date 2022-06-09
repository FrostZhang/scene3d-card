
import json, os, shutil, hashlib, base64
import urllib,urllib.request

class ApiConfig():

    def __init__(self, _dir):        
        if os.path.exists(_dir) == False:           
            self.mkdir(_dir)
        self.dir = _dir

    # 创建文件夹
    def mkdir(self, path):
        folders = []
        while not os.path.isdir(path):
            path, suffix = os.path.split(path)
            folders.append(suffix)
        for folder in folders[::-1]:
            path = os.path.join(path, folder)
            os.mkdir(path)
    
        # 获取路径
    def get_path(self, name):
        return self.dir + '/' + name

    def change_type(byte):    
        if isinstance(byte,bytes):
            return str(byte,encoding="utf-8")  
        return json.JSONEncoder.default(byte)

        # 写入文件内容
    def write(self, name, obj):
        with open(self.get_path(name), 'w', encoding='utf-8') as f:
            json.dump(obj, f, ensure_ascii=False)
        
    def writecustomconfig(self, call):
        self.write('houseconnfig.json', str(call.data))
    
    #data = {'url': 'http://www.blog.pythonlibrary.org/wp-content/uploads/2012/06/', 'supplier': 'asher', 'astype': 'appliances', 'name': 'wxDbViewer', "version": '1.0'}
    def supplier3ddownload(self, call):
        if 'supplier' in data:
            _supplier = data["supplier"]
        if 'astype' in data:
            _astype = data["astype"]
        if 'url' in data:
            _url = data["url"]
        if 'name' in data:
            _name = data["name"]
        if _url == None or _name == None or _supplier==None or _astype==None:
            print('缺数据')
            return
        dirpath = self.dir + '/' + _supplier + '/' + _astype 
        self.mkdir(dirpath)
        png = _name + '.png'
        asdata = _name + '_' + _astype + '.asherlinkdata'
        asmf = _name+'_'+_astype+'.asherlinkdata.manifest'
        urllib.request.urlretrieve(_url + png, dirpath + '/' + png)
        urllib.request.urlretrieve(_url+asdata,dirpath+ '/'+asdata)
        urllib.request.urlretrieve(_url+asmf,dirpath+ '/'+asmf)
