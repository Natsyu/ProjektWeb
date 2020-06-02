﻿using Microsoft.EntityFrameworkCore;
using ProjektWeb.Data.Entities;
using ProjektWeb.Data.Models.Database;
using ProjektWeb.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ProjektWeb.Services
{
    public class DatabaseService : IDatabaseService
    {
        private DatabaseContext databaseContext;
        public DatabaseService(DatabaseContext context)
        {
            databaseContext = context;
        }

        public List<Element> GetAllElements()
        {
            return databaseContext.Elements.ToList();
        }
        
        public IQueryable<Element> GetLazyAllElements()
        {
            return databaseContext.Elements;
        }

        public Element GetElementById(int id)
        {
            return databaseContext.Elements.Where(x => x.Id == id).FirstOrDefault();
        }

        public Element GetElementByName(string title)
        {
            return databaseContext.Elements.Where(x => x.Title == title).FirstOrDefault();
        }

        public Task<Element> AddElement(Element element)
        {
            databaseContext.Elements.Add(element);
            databaseContext.SaveChanges();
            return databaseContext.Elements.Where(e => e.Id == element.Id).FirstOrDefaultAsync();
        }

        public void AddTagToElementById(int elementId, string tag)
        {
            databaseContext.Tags.Add(new Tag() { ElementId = elementId, Name = tag.ToLower()});
            databaseContext.SaveChanges();
        }

        public IEnumerable<Rate> GetAllRates()
        {
            return databaseContext.Rates.ToList();
        }

        public IEnumerable<Rate> GetRatesByAuthor(string author)
        {
            return databaseContext.Rates.Where(x => x.Author == author).ToList();
        }

        public IEnumerable<Rate> GetRatesByElementId(int elementId)
        {
            return databaseContext.Rates.Where(x => x.ElementId == elementId).ToList();
        }

        public IEnumerable<Tag> GetTagsByElementId(int elementId)
        {
            return databaseContext.Tags.Where(x => x.ElementId == elementId).ToList();
        }

        public IEnumerable<Element> GetElementsContainingTag(string tag)
        {
            List<int> elementIds = databaseContext.Tags.Where(x => x.Name == tag).Select(x => x.ElementId).ToList();
            return databaseContext.Elements.Where(x => elementIds.Contains(x.Id)).ToList();
        }

        public IEnumerable<string> GetAllTags()
        {
            List<string> tags = new List<string>();
            databaseContext.Tags.ToList().ForEach(x => tags.Add(x.Name));
            return tags.Distinct();
        }

        public IQueryable<User> AuthenticateUser(string email, string password)
        {
            return databaseContext.Users.Where(u => u.NormalizedEmail == email.ToUpper() && u.Password == Security.HashPassword(password));
        }

        public IQueryable<User> GetUserById(int id)
        {
            return databaseContext.Users.Where(u => u.Id == id);
        }
        public IQueryable<User> GetUserByEmail(string email)
        {
            return databaseContext.Users.Where(u => u.NormalizedEmail == email.ToUpper());
        }

        public IQueryable<User> AddUser(User user)
        {
            databaseContext.Add(user);
            databaseContext.SaveChanges();
            return GetUserByEmail(user.Email);
        }
    }
}
