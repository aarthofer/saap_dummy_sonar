using Apollo.Core.Dal.Common;
using Apollo.Core.Dal.Dao.MappingTables;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Apollo.Core.Dal.Dao
{
    public class CinemaHallDao : ApolloDao<CinemaHall>, ICinemaHallDao
    {
        public CinemaHallDao(IConnectionFactory connectionFactory, IQueryBuilderFactory qbFactory) : base(connectionFactory, qbFactory) { }

        public async Task<IEnumerable<CinemaHall>> FindCinemaHallsByCinemaId(int cinemaId)
        {
            IEnumerable<CinemaHall> cinemaHalls = await base.FindByColumn("cinemaId", cinemaId);

            foreach (CinemaHall cinemaHall in cinemaHalls)
            {
                await FillCinemaHall(cinemaHall);
            }

            return cinemaHalls;
        }

        public override async Task<CinemaHall> FindByIdAsync(int id)
        {
            CinemaHall cinemaHall = await base.FindByIdAsync(id);

            if (cinemaHall == null)
            {
                return cinemaHall;
            }

            await FillCinemaHall(cinemaHall);

            return cinemaHall;
        }

        private async Task FillCinemaHall(CinemaHall cinemaHall)
        {
            var queryBuilder = GetQueryBuilder()
                .Table(typeof(CinemaHallSeat))
                .SetCondition(new Column(typeof(CinemaHallSeat), "cinemaHallId"), OperationType.Equals, cinemaHall.Id);

            cinemaHall.Seats =
                await template.QueryAsync<CinemaHallSeat>(queryBuilder, template.GenericRowMapper<CinemaHallSeat>);
        }

        public override async Task<CinemaHall> CreateAsync(CinemaHall cinemaHall)
        {
            await CheckCinemaHallNotExists(cinemaHall);
            using (TransactionScope scope = new TransactionScope())
            {
                CinemaHall newCinema = await base.CreateAsync(cinemaHall);

                await UpdateSeatList(newCinema, cinemaHall.Seats);

                scope.Complete();

                return newCinema;
            }
        }

        public override async Task<CinemaHall> UpdateAsync(CinemaHall cinemaHall)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                if (cinemaHall.Id == -1)
                {
                    return await CreateAsync(cinemaHall);
                }

                await RemoveNotNeededSeats(cinemaHall);

                CinemaHall updatedCinema = await base.UpdateAsync(cinemaHall);
                updatedCinema.Seats = cinemaHall.Seats;

                await UpdateSeatList(updatedCinema, cinemaHall.Seats);
                scope.Complete();
            }

            return await FindByIdAsync(cinemaHall.Id);
        }

        private async Task<int> RemoveNotNeededSeats(CinemaHall cinemaHall)
        {
            var qb = GetQueryBuilder()
                    .Table(typeof(CinemaHallSeat))
                    .SetCondition(Column.Create<CinemaHallSeat>("cinemaHallId"), OperationType.Equals, cinemaHall.Id);

            var seats = await template.QueryAsync<CinemaHallSeat>(qb);
            seats = seats.Where(seat => cinemaHall.Seats.Where(cinemaSeat => cinemaSeat.Id == seat.Id).Count() == 0);

            if (seats.Count() == 0) {
                return 0;
            }

            var deleteResSeatQB = GetQueryBuilder()
                .Table(typeof(ReservationSeat))
                .QueryType(QueryTypeEnum.DELETE);

            var deleteHallSeatQB = GetQueryBuilder()
                .Table(typeof(CinemaHallSeat))
                .QueryType(QueryTypeEnum.DELETE);
            
            int counter = 0;
            foreach (CinemaHallSeat seat in seats)
            {
                deleteResSeatQB.AddOr(Column.Create<ReservationSeat>("hallSeatId"), OperationType.Equals, seat.Id, $"id{counter}");
                deleteHallSeatQB.AddOr(Column.Create<CinemaHallSeat>("id"), OperationType.Equals, seat.Id, $"id{counter++}");
            }

            int affectedRows = 0;
            
            affectedRows += await template.ExecuteAsync(deleteResSeatQB);
            affectedRows += await template.ExecuteAsync(deleteHallSeatQB);
            return affectedRows;
        }

        public override async Task<int> DeleteByIdAsync(int id)
        {
            return await DeleteCinemaHallAsync(await FindByIdAsync(id));
        }

        public async Task<int> DeleteCinemaHallAsync(CinemaHall cinema)
        {
            int affectedRows = 0;
            var queryBuilder = GetQueryBuilder()
                .QueryType(QueryTypeEnum.DELETE)
                .Table(typeof(CinemaHallSeat))
                .SetCondition(Column.Create<CinemaHallSeat>("cinemaHallId"), OperationType.Equals, cinema.Id);

            using (TransactionScope scope = new TransactionScope())
            {
                affectedRows += await template.ExecuteAsync(queryBuilder);
                affectedRows += await base.DeleteByIdAsync(cinema.Id);

                scope.Complete();
            }

            return affectedRows;
        }

        public async Task<IEnumerable<CinemaHallSeat>> GetCinemaHallSeats(int cinemaHallId)
        {
            var qb = GetQueryBuilder()
                .Table(typeof(CinemaHallSeat))
                .SetCondition(Column.Create<CinemaHallSeat>("cinemaHallId"), OperationType.Equals, cinemaHallId);

            return await template.QueryAsync<CinemaHallSeat>(qb);
        }

        public async Task<CinemaHallSeat> SaveCinemaHallSeat(CinemaHallSeat seat)
        {
            CheckValidSeat(seat);

            await CheckValidSeatPosition(seat);

            //seat is new
            if (seat.Id <= 0)
            {
                return await CreateAsync<CinemaHallSeat>(seat);
            }

            //seat already exists   
            else
            {
                return await UpdateAsync<CinemaHallSeat>(seat);
            }
        }

        public async Task<IEnumerable<CinemaHallSeat>> ResetAllSeats(int cinemaHallId, IEnumerable<CinemaHallSeat> seats)
        {
            CinemaHall cinema = await base.FindByIdAsync(cinemaHallId);
            if (cinema == null)
            {
                throw new ArgumentException("Cinema not found: " + cinemaHallId);
            }

            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    IEnumerable<CinemaHallSeat> tmpSeats = seats.Select(s => s.Clone());
                    foreach (var seat in tmpSeats.Where(s => s.CategoryId == -1))
                    {
                        var queryBuilder = GetQueryBuilder()
                            .QueryType(QueryTypeEnum.DELETE)
                            .Table(typeof(CinemaHallSeat))
                            .SetCondition(Column.Create<CinemaHallSeat>("cinemaHallId"), OperationType.Equals, cinemaHallId);
                        await template.ExecuteAsync(queryBuilder);
                    }

                    foreach (var seat in tmpSeats)
                    {
                        seat.Id = -1;
                    }

                    await UpdateSeatList(cinema, tmpSeats.Where(s => s.CategoryId != -1));
                    transaction.Complete();
                }
            }
            catch (Exception ex)
            {
                // try storing seats in two differen ways
                using (TransactionScope transaction = new TransactionScope())
                {
                    await UpdateSeatList(cinema, seats.Where(s => s.CategoryId != -1));
                    transaction.Complete();
                }
            }


            return await GetCinemaHallSeats(cinemaHallId);
        }

        private async Task UpdateSeatList(CinemaHall cinemaHall, IEnumerable<CinemaHallSeat> seats)
        {
            var categories = await FindAllAsync<SeatCategory>();
            var newSeats = new List<CinemaHallSeat>();

            foreach (var seat in seats)
            {
                //set seat cinemaHallID to cinema.Id in case user configured it wrong
                seat.cinemaHallId = cinemaHall.Id;

                //Update or insert
                var newSeat = await SaveCinemaHallSeat(seat);

                newSeats.Add(newSeat);
            }

            cinemaHall.Seats = newSeats;
        }

        private void CheckValidSeat(CinemaHallSeat seat)
        {
            if (seat.cinemaHallId == -1)
            {
                throw new Exception("CinemaId has to be set when adding a seat");
            }

            if (seat.CategoryId == -1)
            {
                throw new Exception("CategoryId has to be set when adding a seat");
            }
        }

        private async Task CheckValidSeatPosition(CinemaHallSeat seat)
        {
            var qb = GetQueryBuilder()
                .Table(typeof(CinemaHallSeat))
                .SetCondition(Column.Create<CinemaHallSeat>("cinemaHallId"), OperationType.Equals, seat.cinemaHallId)
                .AddAnd(Column.Create<CinemaHallSeat>("id"), OperationType.NotEquals, seat.Id);

            var conditionSeatPos = qb.GetNewCondition()
                .AddAnd(Column.Create<CinemaHallSeat>("row"), OperationType.Equals, seat.Row)
                .AddAnd(Column.Create<CinemaHallSeat>("col"), OperationType.Equals, seat.Col);

            var conditionSeatNr = qb.GetNewCondition()
                .AddAnd(Column.Create<CinemaHallSeat>("seatNr"), OperationType.Equals, seat.SeatNr)
                .AddOr(conditionSeatPos);

            qb.AddAnd(conditionSeatNr);

            if ((await template.QueryAsync<CinemaHallSeat>(qb)).Count() != 0)
            {
                throw new ArgumentException("A seat in the given Row and Column or SeatNr already exists for this cinema. ");
            }
        }

        private async Task CheckCinemaHallNotExists(CinemaHall cinemaHall)
        {
            var qb = GetQueryBuilder()
                .Table(typeof(CinemaHall))
                .SetCondition(Column.Create<CinemaHall>("name"), OperationType.Like, cinemaHall.Name)
                .AddAnd(Column.Create<CinemaHall>("cinemaId"), OperationType.Equals, cinemaHall.CinemaId);

            if ((await template.QueryAsync<CinemaHall>(qb)).Count() != 0)
            {
                throw new ArgumentException("A cinema hall with this name already exists");
            }
        }
    }
}
