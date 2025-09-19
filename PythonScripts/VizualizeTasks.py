import sys
import json
import matplotlib.pyplot as plt

# Get data from .NET via argv
data = json.loads(sys.argv[1])

dates = [item['date'] for item in data]
counts = [item['count'] for item in data]

plt.figure(figsize=(8, 5))
plt.bar(dates, counts, color='skyblue')
plt.title("Tasks Completed Per Day")
plt.xlabel("Date")
plt.ylabel("Tasks")
plt.xticks(rotation=30)
plt.tight_layout()

output_path = "wwwroot/output_chart.png"
plt.savefig(output_path)
print(output_path)

