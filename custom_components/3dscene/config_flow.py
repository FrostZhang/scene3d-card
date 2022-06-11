"""Config flow for Hello World integration."""
#import logging

#import voluptuous as vol

from homeassistant import config_entries

DOMAIN = '3dscene'

class ConfigFlow(config_entries.ConfigFlow, domain=DOMAIN):

    VERSION = 1

    async def async_step_user(self, import_info=None):
        if self._async_current_entries():
            return self.async_abort(reason="single_instance_allowed")
        if self.hass.data.get(DOMAIN):
            return self.async_abort(reason="single_instance_allowed")
        return self.async_create_entry(title="Homeassistant 3D", data={})
    
    async def async_step_import(self, user_input):
        """Handle import."""
        return await self.async_step_user(user_input)