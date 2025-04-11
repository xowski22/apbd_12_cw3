namespace ConsoleApp1.Models;

public class EmpDeptSalgradeTests
{
    // 1. Find employees hired in 1981
    [Fact]
    public void ShouldFindEmployeesHiredIn1981()
    {
        var emps = Database.GetEmps();

        var result = emps.Where(e => e.HireDate.Year == 1981).ToList();

        Assert.Equal(4, result.Count);
        Assert.All(result, e => Assert.Equal(1981, e.HireDate.Year));
    }

    // 2. Find top 3 earners
    [Fact]
    public void ShouldFindTopThreeEarners()
    {
        var emps = Database.GetEmps();

        var result = emps.OrderByDescending(e => e.Sal).Take(3).ToList();

        Assert.Equal(3, result.Count);
        Assert.Equal(5000, result[0].Sal);
    }

    // 3. Group employees by job title and count them
    [Fact]
    public void ShouldGroupEmployeesByJobTitle()
    {
        var emps = Database.GetEmps();

        var result = emps.GroupBy(e => e.Job)
                         .Select(g => new { Job = g.Key, Count = g.Count() })
                         .OrderByDescending(g => g.Count)
                         .ToList();

        Assert.Contains(result, r => r.Job == "SALESMAN" && r.Count == 2);
    }

    // 4. Find employees with names containing the letter 'I'
    [Fact]
    public void ShouldFindEmployeesWithNamesContainingLetterI()
    {
        var emps = Database.GetEmps();

        var result = emps.Where(e => e.EName.Contains('I')).ToList();

        Assert.Contains(result, e => e.EName == "SMITH");
        Assert.Contains(result, e => e.EName == "KING");
    }

    // 5. Calculate total company salary budget
    [Fact]
    public void ShouldCalculateTotalSalaryBudget()
    {
        var emps = Database.GetEmps();

        decimal totalSalary = emps.Sum(e => e.Sal);

        Assert.Equal(13650, totalSalary);
    }

    // 6. Find the department with the highest average salary
    [Fact]
    public void ShouldFindDepartmentWithHighestAverageSalary()
    {
        var emps = Database.GetEmps();

        var result = emps.GroupBy(e => e.DeptNo)
                         .Select(g => new { DeptNo = g.Key, AvgSalary = g.Average(e => e.Sal) })
                         .OrderByDescending(g => g.AvgSalary)
                         .First();

        Assert.Equal(10, result.DeptNo);
        Assert.Equal(5000, result.AvgSalary);
    }

    // 7. Find employees who are managers of other employees
    [Fact]
    public void ShouldFindManagerEmployees()
    {
        var emps = Database.GetEmps();

        var managerIds = emps.Where(e => e.Mgr.HasValue)
                             .Select(e => e.Mgr.Value)
                             .Distinct();
                             
        var managers = emps.Where(e => managerIds.Contains(e.EmpNo)).ToList();

        Assert.Contains(managers, e => e.EName == "FORD");
        Assert.Contains(managers, e => e.EName == "KING");
    }

    // 8. Find employees who earn more than their managers
    [Fact]
    public void ShouldFindEmployeesEarningMoreThanTheirManagers()
    {
        var emps = Database.GetEmps();

        var result = from e in emps
                     where e.Mgr.HasValue
                     join manager in emps on e.Mgr equals manager.EmpNo
                     where e.Sal > manager.Sal
                     select e.EName;

        // In the sample data, there are no employees who earn more than their managers
        Assert.Empty(result);
    }

    // 9. Calculate average length of employment in years
    [Fact]
    public void ShouldCalculateAverageLengthOfEmployment()
    {
        var emps = Database.GetEmps();
        DateTime currentDate = new DateTime(2023, 1, 1); // Reference date

        var averageYears = emps.Average(e => (currentDate - e.HireDate).TotalDays / 365.25);

        Assert.True(averageYears > 40); // Hired in 1980-1981, so over 40 years by 2023
    }

    // 10. Partition employees into salary brackets
    [Fact]
    public void ShouldPartitionEmployeesIntoSalaryBrackets()
    {
        var emps = Database.GetEmps();

        var brackets = new[]
        {
            new { Name = "Low", Min = 0M, Max = 1000M },
            new { Name = "Medium", Min = 1001M, Max = 3000M },
            new { Name = "High", Min = 3001M, Max = decimal.MaxValue }
        };

        var result = from bracket in brackets
                     join emp in emps
                     on true equals true
                     where emp.Sal > bracket.Min && emp.Sal <= bracket.Max
                     group emp by bracket.Name into g
                     select new { Bracket = g.Key, Count = g.Count() };

        Assert.Contains(result, r => r.Bracket == "Low" && r.Count == 1);
        Assert.Contains(result, r => r.Bracket == "Medium" && r.Count == 2);
        Assert.Contains(result, r => r.Bracket == "High" && r.Count == 2);
    }
    
    // 11. Find employees with consecutive hire dates
    [Fact]
    public void ShouldFindEmployeesWithConsecutiveHireDates()
    {
        var emps = Database.GetEmps();

        var orderedByHireDate = emps.OrderBy(e => e.HireDate).ToList();
        
        var consecutivePairs = orderedByHireDate
            .Zip(orderedByHireDate.Skip(1), (first, second) => new { 
                First = first, 
                Second = second,
                DaysDifference = (second.HireDate - first.HireDate).TotalDays
            })
            .Where(pair => pair.DaysDifference <= 3)
            .ToList();

        Assert.Contains(consecutivePairs, p => p.First.EName == "ALLEN" && p.Second.EName == "WARD");
    }

    // 12. Calculate commission percentage of salary
    [Fact]
    public void ShouldCalculateCommissionPercentage()
    {
        var emps = Database.GetEmps();

        var result = emps.Where(e => e.Comm.HasValue && e.Comm > 0)
                        .Select(e => new { 
                            e.EName, 
                            e.Sal, 
                            e.Comm, 
                            CommPercentage = Math.Round(e.Comm.Value / e.Sal * 100, 2) 
                        })
                        .OrderByDescending(e => e.CommPercentage)
                        .ToList();

        Assert.Contains(result, r => r.EName == "WARD" && r.CommPercentage == 40);
        Assert.Contains(result, r => r.EName == "ALLEN" && r.CommPercentage == 18.75);
    }

    // 13. Find employees with the same job in each department
    [Fact]
    public void ShouldFindSameJobsAcrossDepartments()
    {
        var emps = Database.GetEmps();

        var jobsInMultipleDepts = emps.GroupBy(e => e.Job)
                                     .Where(g => g.Select(e => e.DeptNo).Distinct().Count() > 1)
                                     .Select(g => g.Key)
                                     .ToList();

        Assert.Contains("CLERK", jobsInMultipleDepts);
    }

    // 14. Find employees with the longest names
    [Fact]
    public void ShouldFindEmployeesWithLongestNames()
    {
        var emps = Database.GetEmps();

        int maxLength = emps.Max(e => e.EName.Length);
        
        var longestNames = emps.Where(e => e.EName.Length == maxLength)
                              .Select(e => e.EName)
                              .ToList();

        Assert.Contains("SMITH", longestNames);
        Assert.Contains("ALLEN", longestNames);
    }

    // 15. Calculate ratio of managers to employees
    [Fact]
    public void ShouldCalculateManagerToEmployeeRatio()
    {
        var emps = Database.GetEmps();

        var managerIds = emps.Where(e => e.Mgr.HasValue)
                            .Select(e => e.Mgr.Value)
                            .Distinct()
                            .ToList();
                            
        int managerCount = emps.Count(e => managerIds.Contains(e.EmpNo));
        int employeeCount = emps.Count;
        
        double ratio = (double)managerCount / employeeCount;

        Assert.True(ratio > 0.2 && ratio < 0.5); // Assertion based on sample data
    }

    // 16. Generate employee report with salary quartile
    [Fact]
    public void ShouldCalculateSalaryQuartiles()
    {
        var emps = Database.GetEmps();

        // Calculate quartile boundaries
        var sortedSalaries = emps.Select(e => e.Sal).OrderBy(s => s).ToList();
        int count = sortedSalaries.Count;
        
        decimal q1 = sortedSalaries[(int)(count * 0.25)];
        decimal q2 = sortedSalaries[(int)(count * 0.5)];
        decimal q3 = sortedSalaries[(int)(count * 0.75)];
        
        var report = emps.Select(e => new {
            e.EName,
            e.Sal,
            Quartile = e.Sal <= q1 ? 1 :
                       e.Sal <= q2 ? 2 :
                       e.Sal <= q3 ? 3 : 4
        }).ToList();
        
        Assert.Equal(4, report.Select(r => r.Quartile).Distinct().Count());
    }

    // 17. Compute salary increase budget by department
    [Fact]
    public void ShouldComputeSalaryIncreaseBudget()
    {
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();
        
        // Calculate 5% salary increase budget per department
        var budgets = from dept in depts
                      join emp in emps on dept.DeptNo equals emp.DeptNo
                      group emp by new { dept.DeptNo, dept.DName } into g
                      let totalSal = g.Sum(e => e.Sal)
                      let increaseBudget = totalSal * 0.05m
                      select new {
                          g.Key.DeptNo,
                          g.Key.DName,
                          TotalSalary = totalSal,
                          IncreaseBudget = increaseBudget
                      };
                      
        var results = budgets.ToList();
        
        Assert.Contains(results, r => r.DName == "SALES" && r.IncreaseBudget == 142.5m);
    }

    // 18. Find departments with salary disparity
    [Fact]
    public void ShouldFindDepartmentsWithSalaryDisparity()
    {
        var emps = Database.GetEmps();
        
        var disparities = emps.GroupBy(e => e.DeptNo)
                            .Where(g => g.Count() >= 2) // Need at least 2 employees
                            .Select(g => new {
                                DeptNo = g.Key,
                                MaxSal = g.Max(e => e.Sal),
                                MinSal = g.Min(e => e.Sal),
                                Ratio = g.Max(e => e.Sal) / g.Min(e => e.Sal)
                            })
                            .OrderByDescending(d => d.Ratio)
                            .ToList();
        
        Assert.Contains(disparities, d => d.DeptNo == 30 && d.Ratio > 1);
    }

    // 19. Create hierarchical organization chart
    [Fact]
    public void ShouldCreateOrganizationHierarchy()
    {
        var emps = Database.GetEmps();
        
        // Find top-level managers (those without managers)
        var topManagers = emps.Where(e => e.Mgr == null).ToList();
        
        // For each top manager, create a hierarchy
        var orgChart = topManagers.Select(tm => new {
            Manager = tm.EName,
            DirectReports = emps.Where(e => e.Mgr == tm.EmpNo)
                               .Select(dr => new {
                                   Employee = dr.EName,
                                   SecondLevel = emps.Where(e => e.Mgr == dr.EmpNo)
                                                   .Select(sl => sl.EName)
                                                   .ToList()
                               }).ToList()
        }).ToList();
        
        Assert.Single(orgChart); // Only one top manager
        Assert.Equal("KING", orgChart.First().Manager);
    }

    // 20. Calculate department efficiency (salary per employee)
    [Fact]
    public void ShouldCalculateDepartmentEfficiency()
    {
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();
        
        var efficiency = from dept in depts
                         join emp in emps on dept.DeptNo equals emp.DeptNo
                         group emp by new { dept.DeptNo, dept.DName } into g
                         let totalSalary = g.Sum(e => e.Sal)
                         let employeeCount = g.Count()
                         select new {
                             g.Key.DeptNo,
                             g.Key.DName,
                             EmployeeCount = employeeCount,
                             TotalSalary = totalSalary,
                             AverageSalary = totalSalary / employeeCount,
                             // Lower average salary could mean more efficiency
                             EfficiencyRating = 1000 / (totalSalary / employeeCount)
                         };
        
        var results = efficiency.OrderByDescending(e => e.EfficiencyRating).ToList();
        
        // Department with lowest average salary per employee would have highest efficiency
        Assert.Equal(30, results.First().DeptNo);
    }
}