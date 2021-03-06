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
using System.Transactions;

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

        public IQueryable<Element> GetElementById(int id)
        {
            return databaseContext.Elements.Where(x => x.Id == id);
        }

        public Element GetElementByName(string title)
        {
            return databaseContext.Elements.Where(x => x.Title.ToUpper() == title.ToUpper()).FirstOrDefault();
        }

        public Task<Element> AddElement(Element element)
        {
            databaseContext.Elements.Add(element);
            databaseContext.SaveChanges();
            return databaseContext.Elements.Where(e => e.Id == element.Id).FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteElementById(int id)
        {
            var elementToDelete = GetElementById(id).FirstOrDefault();

            if (elementToDelete == null)
                return false;

            using (var transaction = databaseContext.Database.BeginTransaction())
            {
                try
                {
                    databaseContext.Remove(elementToDelete);
                    await databaseContext.SaveChangesAsync();

                    transaction.Commit();
                    return true;

                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }

        }

        public void AddTagToElementById(int elementId, string tag)
        {
            databaseContext.Tags.Add(new Tag() { ElementId = elementId, Name = tag.ToLower() });
            databaseContext.SaveChanges();
        }

        public IEnumerable<Rate> GetAllRates()
        {
            return databaseContext.Rates.ToList();
        }

        public IEnumerable<Rate> GetRatesByAuthor(int author)
        {
            return databaseContext.Rates.Where(x => x.Author == author && !x.IsDeleted).ToList();
        }

        public IEnumerable<Rate> GetRatesByElementId(int elementId)
        {
            return databaseContext.Rates.Where(x => x.ElementId == elementId && !x.IsDeleted).ToList();
        }

        public IEnumerable<Tag> GetTagsByElementId(int elementId)
        {
            return databaseContext.Tags.Where(x => x.ElementId == elementId && !x.IsDeleted).ToList();
        }

        public IEnumerable<Element> GetElementsContainingTag(string tag)
        {
            List<int> elementIds = databaseContext.Tags.Where(x => x.Name.ToUpper() == tag.ToUpper() && !x.IsDeleted).Select(x => x.ElementId).ToList();
            return databaseContext.Elements.Where(x => elementIds.Contains(x.Id)).ToList();
        }

        public IEnumerable<string> GetAllTags()
        {
            List<string> tags = new List<string>();
            databaseContext.Tags.Where(x => !x.IsDeleted).ToList().ForEach(x => tags.Add(x.Name));
            return tags.Distinct();
        }

        public IQueryable<User> AuthenticateUser(string email, string password)
        {
            User user = databaseContext.Users.Where(u => u.NormalizedEmail == email.ToUpper()).FirstOrDefault();
            return databaseContext.Users.Where(u => u.NormalizedEmail == email.ToUpper() && u.Password == Security.HashPassword(password, u.Salt) && !u.IsDeleted);
        }

        public IQueryable<User> GetUserById(int id)
        {
            return databaseContext.Users.Where(u => u.Id == id && !u.IsDeleted);
        }
        public IQueryable<User> GetUserByEmail(string email)
        {
            return databaseContext.Users.Where(u => u.NormalizedEmail == email.ToUpper() && !u.IsDeleted);
        }

        public IQueryable<User> AddUser(User user)
        {
            databaseContext.Add(user);
            databaseContext.SaveChanges();
            return GetUserByEmail(user.NormalizedEmail);
        }

        public async Task<bool> DeleteUserById(int id)
        {
            var userToDelete = GetUserById(id).FirstOrDefault();

            if (userToDelete == null)
                return false;

            using (var transaction = databaseContext.Database.BeginTransaction())
            {
                try
                {
                    userToDelete.IsDeleted = true;
                    databaseContext.Update(userToDelete);
                    await databaseContext.SaveChangesAsync();

                    transaction.Commit();
                    return true;

                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }

        }
        public IQueryable<Element> UpdateElement(Element element)
        {
            databaseContext.Elements.Update(element);
            databaseContext.SaveChanges();
            return databaseContext.Elements.Where(e => e.Id == element.Id);
        }

        public Task<Rate> AddRate(Rate rate)
        {
            databaseContext.Add(rate);
            databaseContext.SaveChanges();
            return Task.FromResult(GetRatesByAuthor(rate.Author).Where(x => x.ElementId == rate.ElementId && x.IsDeleted == false).FirstOrDefault());
        }

        public Task<Rate> UpdateRate(Rate rate)
        {
            databaseContext.Rates.Update(rate);
            databaseContext.SaveChanges();
            return Task.FromResult(GetRatesByAuthor(rate.Author).Where(x => x.ElementId == rate.ElementId && x.IsDeleted == false).FirstOrDefault());
        }
        public Task<int> GetElementCount()
        {
            return databaseContext.Elements.Where(x => !x.IsDeleted).CountAsync();
        }

    }
}
