using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniNFe.Database;
using UniNFe.Database.Models;

namespace UniNFe.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {

        private UniNFeContext _uniNFeContext; 

        public ConfigurationController()
        {
            _uniNFeContext = new UniNFeContext();
        }

        [HttpPost]
        public ActionResult<Configuration> Post(Configuration configuration)
        {
            var configurations = _uniNFeContext.Configurations.ToList();
            if (configurations.Count == 0)
            {
                configuration.Id = Guid.NewGuid(); 
                _uniNFeContext.Configurations.Add(configuration);
                _uniNFeContext.SaveChanges();
                return Ok(configuration);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPut("{id}")]
        public Configuration Put(Guid id, Configuration configurationPayload)
        {
            var configurationUpdate = _uniNFeContext.Configurations.FirstOrDefault(configuration => configuration.Id == id);
            configurationUpdate.Name = configurationPayload.Name;
            configurationUpdate.DocumentType = configurationPayload.DocumentType;
            configurationUpdate.DocumentNumber = configurationPayload.DocumentNumber;
            configurationUpdate.Email = configurationPayload.Email;
            configurationUpdate.Service = configurationPayload.Service;
            configurationUpdate.Csc = configurationPayload.Csc;
            configurationUpdate.IdToken = configurationPayload.IdToken;
            configurationUpdate.UF = configurationPayload.UF;
            configurationUpdate.Environment = configurationPayload.Environment;
            configurationUpdate.IssuanceType = configurationPayload.IssuanceType;
            _uniNFeContext.Update(configurationUpdate);
            _uniNFeContext.SaveChanges();
            return configurationUpdate;
        }

        [HttpGet]
        public IList<Configuration> Get()
        {
            return _uniNFeContext.Configurations.ToList();
        }

        [HttpGet("{id}")]
        public Configuration Get(Guid id)
        {
            return _uniNFeContext.Configurations.FirstOrDefault(configuration => configuration.Id == id);
        }
    }
}
