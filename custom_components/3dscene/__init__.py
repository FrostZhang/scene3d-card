
import homeassistant
import json, os, shutil, hashlib, base64
from .api_config import ApiConfig
from homeassistant.config_entries import ConfigEntry
from homeassistant.core import HomeAssistant
DOMAIN = "3dscene"

async def async_setup(hass, config):
    # 注册静态目录
    VERSION = '1.0.0'
    ROOT_PATH = '/3dscene_local'
    #await hass.http.async_register_static_paths([StaticPathConfig("/3dscene_local", "/config/custom_components/3dscene/local", False)])

    hass.http.register_static_path(ROOT_PATH, hass.config.path('custom_components/3dscene/local'), False)
    hass.components.frontend.add_extra_js_url(hass, ROOT_PATH + '/3dscene-card.js?ver=' + VERSION)

    api_config =  ApiConfig(hass.config.path("custom_components/3dscene/local/Customdata"))
    hass.services.async_register('3dscene', 'configscene', api_config.writeconfigscene)
    hass.services.async_register('3dscene', 'configui', api_config.writeconfigui)
    return True

async def async_setup_entry(hass: HomeAssistant, entry: ConfigEntry) -> bool:
    # 注册静态目录
    return True

async def async_unload_entry(hass: HomeAssistant, entry: ConfigEntry) -> bool:

    return True