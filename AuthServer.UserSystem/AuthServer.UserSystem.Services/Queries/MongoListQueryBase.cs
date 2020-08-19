using AuthServer.UserSystem.Services.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AuthServer.UserSystem.Services.Queries
{
    public abstract class MongoListQueryBase
    {
        protected void AddSortingToQuery<TDocument, TProjection>(IFindFluent<TDocument, TProjection> query, Dictionary<string, Expression<Func<TDocument, object>>> sortDictionary, string propName, bool isDesc)
        {
            Expression<Func<TDocument, object>> sortExp;
            if (!sortDictionary.TryGetValue(propName.ToLower(), out sortExp))
                sortExp = sortDictionary["default"];

            SortDefinition<TDocument> sortDef = null;
            if (isDesc)
                sortDef = Builders<TDocument>.Sort.Descending(sortExp);
            else
                sortDef = Builders<TDocument>.Sort.Ascending(sortExp);
            query.Sort(sortDef);
        }
        protected void AddPagingToQuery<TDocument, TProjection>(IFindFluent<TDocument, TProjection> query, int pageNumber, int pageSize)
        {
            query.Skip((pageNumber - 1) * pageSize).Limit(pageSize);
        }
        protected void ValidateAndCorrectListQueryModel(ListQueryModel queryModel)
        {
            if (queryModel.Page < 1)
                queryModel.Page = 1;
            if (!(new int[] { 2, 10, 20, 50, 100 }.Contains(queryModel.PageSize)))
                queryModel.PageSize = 10;
            if (queryModel.Order == null)
                queryModel.Order = "";
        }
    }
}
