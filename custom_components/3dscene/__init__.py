
import homeassistant
import json, os, shutil, hashlib, base64
from .api_config import ApiConfig

def setup(hass, config):
    # 注册静态目录
    VERSION = '1.0.0'
    ROOT_PATH = '/3dscene_local'
    hass.http.register_static_path(ROOT_PATH, hass.config.path('custom_components/3dscene/local'), False)
    hass.components.frontend.add_extra_js_url(hass, ROOT_PATH + '/3dscene-card.js?ver=' + VERSION)

    api_config =  ApiConfig(hass.config.path("custom_components/3dscene/local/Customdata"))
    hass.services.async_register('3dscene', 'config', api_config.writecustomconfig)
    return True

async def async_setup_entry(hass, config_entry):
    hass.config_entries.async_forward_entry_setup(config_entry)
    return True

