import random as r
import time
import matplotlib
import scipy

matplotlib.use('TkAgg')
import matplotlib.pyplot as plt


# converting a decision plan to a valid output according to the paper
def print_optimal(optimal_path, n):
    optimal_path.sort(key=lambda a: a[0])

    solution_string = ""
    for (day, f) in optimal_path:
        n -= int(f)

        solution_string += f"{f}, {n}\n"
    return solution_string


# function that makes the aggregate tuples
def calculate_aggregate_tuples(input):
    aggregate_tuples = []
    total_h = 0

    for day, (s, p, h) in enumerate(input):
        aggregate_tuples.append((p + total_h, s, day))

        # keep an aggregate hotel price so no extra looping is necessary
        total_h += h

    return aggregate_tuples


# the function that solves the problem
def solve_strike(instance, n):
    # calculate and sort tuples
    tuples = calculate_aggregate_tuples(instance)
    tuples.sort(key=lambda a: a[0])

    cost = 0
    passengers_left = n

    # keeping track of the decisions made
    optimal_path = []

    # loop through the sorted tuples until no people are left
    m = len(instance)
    for idx in range(m):
        (a, s, day) = tuples[idx]

        passengers_leaving = min(s, passengers_left)

        cost += a * passengers_leaving
        passengers_left -= passengers_leaving

        optimal_path.append((day, passengers_leaving))

    return cost, print_optimal(optimal_path, n)


# generating a lot of instances before computing their outcomes
def generate_instance_map(n, step_size, test_size):
    all_instances = {}

    for m in range(step_size, test_size + 1, step_size):
        # make an instance of length m with n passengers

        if m % (test_size // 100) == 0:
            print(
                f"generating instance of size: {m} days. ({round(m / test_size * 100, 1)} %)")

        inst = make_instance(m, n)
        all_instances[m] = inst
    return all_instances


# making a random instance for m days and n passengers
def make_instance(length=3, passengers=100, max_seat_price=100, max_hotel_price=100):
    instance = []
    total_s = 0

    for i in range(length):
        s = r.randint(1, passengers)
        p = r.randint(1, max_seat_price)
        h = r.randint(1, max_hotel_price)

        total_s += s

        instance.append((s, p, h))

    # ensure each instance is feasible
    if total_s < passengers:
        instance[length - 1] = (passengers, r.randint(1, 100), r.randint(1, 100))

    return instance


# function for generating, solving and plotting random instances
def generate_complexity_graph():
    start_total = time.time()

    # declare n or m as static variable
    n = 100

    duration_list = []
    days_list = []
    passengers_list = []
    cost_list = []

    test_size = 500000  # input size from 5,000 to 500,000 days
    step_size = test_size // 1000  # 100 test cases

    all_instances = generate_instance_map(n, step_size, test_size)

    for m in range(step_size, test_size + 1, step_size):
        if m % (test_size // 100) == 0:
            print(
                f"calculating solution for instance size: {m} days. "
                f"Total duration: {round(time.time() - start_total, 1)} s. "
                f"({round(m / test_size * 100, 1)} %)")

        start = time.time()

        current_instance = all_instances[m]

        cost, _ = solve_strike(current_instance, n)  # solve the instance
        cost_list.append(cost)  # save the cost

        end = time.time()
        duration = end - start

        duration_list.append(duration * 1000)  # save the time in ms
        days_list.append(m)  # save the instance size in days
        passengers_list.append(n)  # save the instance size in passengers

    end_total = time.time()

    print(f"Total duration: {(end_total - start_total) / 60} minutes")

    # get the formula and R^2 values
    slope, intercept, r_value, p_value, std_err = scipy.stats.linregress(days_list, duration_list)

    print(f"slope: {slope} intercept: {intercept}, R^2_value: {r_value**2}, p_value: {p_value}, std_err: {std_err}")
    print([f"{day},{duration}" for (day, duration) in zip(days_list, duration_list)])
    with open('results.csv', 'w') as f:
        f.writelines([f"{day},{duration}\n" for (day, duration) in zip(days_list, duration_list)])

    plt.plot(days_list, duration_list)
    plt.xlabel("Amount of days m")
    plt.ylabel("Computation duration (ms)")
    plt.title("Computation duration compared to amount of days in an instance")

    # this is for making the second plot. Some more modifications need to happen so m stays constant and n is changed
    # plt.xlabel("Amount of people n")
    # plt.ylabel("Computation duration (ms)")
    # plt.title("Computation duration compared to amount of people in an instance")
    plt.show()

    plt.plot(days_list, cost_list)
    plt.xlabel("Amount of days m")
    plt.ylabel("Optimal cost")
    plt.title("Optimal cost compared to amount of days in an instance")
    plt.show()


if __name__ == "__main__":

    # this part generates random functions
    generate_complexity_graph()

    # this part of the code is to test individual cases. The input files need to be in the format described in the paper
    instance = []
    n = 100
    with open("input.txt", "r") as f:
        for i, line in enumerate(f.readlines()):

            if i == 0:  # retrieve n from the input
                n = int(line.strip())
                continue
            elif i == 1:
                continue

            sLine = line.split(", ")

            s = int(sLine[0])
            l = int(sLine[1])
            h = int(sLine[2])

            instance.append((s, l, h))

    start = time.time()

    cost, solution_string = solve_strike(instance, n)

    end = time.time()
    duration = f"{(end - start) * 1000} ms"

    print(f"Outcome:\n{solution_string}Cost:\n{cost}")
    print(f"Duration: {duration}")
