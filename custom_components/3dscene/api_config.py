
import ast
from contextlib import nullcontext
import json, os, shutil, hashlib, base64
from sysconfig import get_path

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
    
    def joinSupplier(self,call):
        if "supplier" in call.data:
            supplier = call.data["supplier"]
        if "user" in call.data:
            user = call.data["user"]
        if "key" in call.data:
            key = call.data["key"]
        if supplier is None or user is None or key is None:
            return
        
        filesupplier = self.get_path('supplier.json')
        if os.path.isfile(filesupplier):
            with open(filesupplier,'w',encoding="UTF-8") as f:
                li = json.load(f)
                for mode in li:
                    dic = json.loads(mode)
                    if "supplier" in dic and dic["supplier"] == supplier:
                        return
                mode = {}
                mode["supplier"] = supplier
                mode["user"] = user
                mode["key"] = key
                li.append(mode)
                json.dump(li, f, ensure_ascii=False)
        else:
            li =[]
            mode = {}
            mode["supplier"] = supplier
            mode["user"] = user
            mode["key"] = key
            li.append(mode)
            with open(filesupplier, 'w', encoding='utf-8') as f:
                json.dump(li, f, ensure_ascii=False)

    def downSupplierresource(self,call):
        if "supplier" in call.data:
            supplier = call.data["supplier"]
        if "name" in call.data:
            name = call.data["name"]
        if "uri" in call.data:
            uri = call.data["uri"]
        if "astype" in call.data:
            astype = call.data["astype"]
        if supplier is None or name is None or uri is None or astype is None:
            return

        dir = self.get_path(supplier)
        if not os.path.isdir(dir):
            os.mkdir(dir)
        dirastype = self.get_path(supplier+'/'+astype)
        if not os.path.isdir(dirastype):
            os.mkdir(dirastype)
        filepng= "/"+name+'.png'
        urllib.request.urlretrieve(uri +filepng,self.get_path(supplier+filepng))
        fileasdata = '/'+name+'_'+astype+'.asherlinkdate'
        urllib.request.urlretrieve(uri +fileasdata,self.get_path(supplier+fileasdata))
        filemanifest = '/'+name+'_'+astype+'.asherlinkdata.manifest'
        urllib.request.urlretrieve(uri +filemanifest,self.get_path(supplier+filemanifest))

        file3dscene = self.get_path(supplier+'/3dscene.json')
        dataname = astype+'/'+name+'_'+astype+'.asherlinkdate'
        if os.path.isfile(file3dscene):
            with open(file3dscene,'w',encoding="UTF-8") as f:
                li = json.load(f)
                for mode in li:
                    dic = json.loads(mode)
                    if "datapath" in dic and dic["datapath"] == dataname:
                        return
                mode = {}
                mode["datapath"] = dataname
                mode["previewpath"] = astype+'/'+name+'.png'
                li.append(mode)
                json.dump(li, f, ensure_ascii=False)
        else:
            li =[]
            mode = {}
            mode["datapath"] = dataname
            mode["previewpath"] = astype+'/'+name+'.png'
            li.append(mode)
            with open(file3dscene, 'w', encoding='utf-8') as f:
                json.dump(li, f, ensure_ascii=False)