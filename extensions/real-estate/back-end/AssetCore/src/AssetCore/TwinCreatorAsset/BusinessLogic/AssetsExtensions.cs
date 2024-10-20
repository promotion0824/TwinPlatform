using System;
using System.Collections.Generic;
using System.Linq;
using AssetCoreTwinCreator.Constants.Schema;
using AssetCoreTwinCreator.Models;
using System.Linq.Expressions;

namespace AssetCoreTwinCreator.BusinessLogic
{
    public static class AssetsExtensions
    {
        public static List<Asset> OrderByAscending(this List<Asset> assets, string sortBy)
        {
            return OrderBy(assets, sortBy);
        }

        public static List<Asset> OrderByDescending(this List<Asset> assets, string sortBy)
        {
            return OrderBy(assets, sortBy, true);
        }

        private static List<Asset> OrderBy(List<Asset> assets, string sortBy, bool reverse = false)
        {
            if(assets == null || assets.Count() == 0)
            {
                return assets;
            }

            if(string.IsNullOrWhiteSpace(sortBy))
            {
                return assets;
            }

            var sortExpression = GetSortExpressionForReservedColumns(sortBy);
            if(sortExpression != null)
            {
                if(reverse == false)
                {
                    return assets.AsQueryable().OrderBy(sortExpression).ToList();
                }
                else
                {
                    return assets.AsQueryable().OrderByDescending(sortExpression).ToList();
                }
            }

            var doesSortByExistInAssetParameters = assets.First().AssetParameters.Any(x => x.Key == sortBy);
            if(doesSortByExistInAssetParameters == false)
            {
                return assets;
            }

            assets.Sort((x, y) =>
            {
                var valueX = x.AssetParameters?.FirstOrDefault(a => a.Key == sortBy)?.Value;
                var valueY = y.AssetParameters?.FirstOrDefault(a => a.Key == sortBy)?.Value;

                var response = CompareObjects(valueX, valueY);

                return reverse == false ? response  : response * -1 ;
            });

            return assets;
        }

        private static Expression<Func<Asset, object>> GetSortExpressionForReservedColumns(string sortBy)
        {
            Expression<Func<Asset, object>> sortExpression = null;
            switch (sortBy)
            {
                case ReservedNames.ColumnNameName:
                    sortExpression = (x => x.Name);
                    break;
                case ReservedNames.ColumnNameSyncDate:
                    sortExpression = (x => x.SyncDate);
                    break;
                case ReservedNames.ColumnNameFloorCode:
                    sortExpression = (x => x.FloorCode);
                    break;
                case ReservedNames.ColumnNameWillowId:
                    sortExpression = (x => x.Id);
                    break;
                case ReservedNames.ColumnLastUpdated:
                    sortExpression = (x => x.UpdatedDate);
                    break;
            }

            return sortExpression;
        }

        private static int CompareObjects(object valueX, object valueY)
        {
            var myValueX = valueX as IComparable;
            var myValueY = valueY as IComparable;

            if (myValueX == null)
            {
                if (myValueY == null)
                {
                    return 0;
                }
                return -1;
            }

            if (myValueY == null)
            {
                return 1;
            }

            var typeX = valueX.GetType();
            var typeY = valueY.GetType();
            if (typeX.Equals(typeY) == false)
            {
                return -1;
            }

            if(typeX == typeof(string))
            {
                return string.Compare(valueX.ToString(), valueY.ToString(), true);
            }

            return myValueX.CompareTo(myValueY);
        }
    }
}